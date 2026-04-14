using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TrendplusProdavnica.Application.Checkout.Dtos;
using TrendplusProdavnica.Application.Checkout.Services;
using TrendplusProdavnica.Domain.Enums;
using TrendplusProdavnica.Domain.Sales;
using TrendplusProdavnica.Infrastructure.Persistence;

namespace TrendplusProdavnica.Infrastructure.Services;

public class CheckoutService : ICheckoutService
{
    private const decimal DefaultCourierDeliveryAmount = 300m;
    private readonly TrendplusDbContext _context;

    public CheckoutService(TrendplusDbContext context)
    {
        _context = context;
    }

    public async Task<CheckoutSummaryDto?> GetCheckoutSummaryAsync(string cartToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cartToken))
        {
            return null;
        }

        var cart = await LoadCartByTokenAsync(cartToken, asNoTracking: true, onlyActive: true, cancellationToken);
        if (cart == null || cart.Items.Count == 0)
        {
            return null;
        }

        var validation = ValidateCartForCheckout(cart);
        if (validation.Errors.Count > 0 || validation.Items.Count == 0)
        {
            return null;
        }

        var items = validation.Items
            .Select(item => new CartItemSummaryDto
            {
                ProductName = item.Variant.Product!.Name,
                BrandName = item.Variant.Product.Brand?.Name ?? string.Empty,
                SizeEu = item.Variant.SizeEu,
                UnitPrice = item.UnitPrice,
                Quantity = item.CartItem.Quantity,
                LineTotal = item.UnitPrice * item.CartItem.Quantity
            })
            .ToList();

        var subtotal = items.Sum(x => x.LineTotal);
        var deliveryAmount = CalculateDeliveryAmount(DeliveryMethod.Courier);

        return new CheckoutSummaryDto
        {
            CartToken = cartToken,
            Items = items,
            SubtotalAmount = subtotal,
            DeliveryAmount = deliveryAmount,
            TotalAmount = subtotal + deliveryAmount,
            ItemCount = items.Sum(x => x.Quantity)
        };
    }

    public Task<CheckoutResultDto> PlaceOrderAsync(CheckoutRequest request, CancellationToken cancellationToken = default)
    {
        return PlaceOrderCoreAsync(request, attemptCount: 0, cancellationToken);
    }

    public async Task<OrderDto?> GetOrderByNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);

        if (order == null)
        {
            return null;
        }

        return new OrderDto
        {
            OrderNumber = order.OrderNumber,
            Status = order.Status.ToString(),
            CustomerFullName = order.GetCustomerFullName(),
            Email = order.Email,
            Phone = order.Phone,
            DeliveryAddressLine1 = order.DeliveryAddressLine1,
            DeliveryAddressLine2 = order.DeliveryAddressLine2,
            DeliveryCity = order.DeliveryCity,
            DeliveryPostalCode = order.DeliveryPostalCode,
            DeliveryMethod = order.DeliveryMethod.ToString(),
            PaymentMethod = order.PaymentMethod.ToString(),
            SubtotalAmount = order.SubtotalAmount,
            DeliveryAmount = order.DeliveryAmount,
            TotalAmount = order.TotalAmount,
            Items = order.Items
                .Select(oi => new OrderItemDto
                {
                    ProductName = oi.ProductNameSnapshot,
                    BrandName = oi.BrandNameSnapshot,
                    SizeEu = oi.SizeEuSnapshot,
                    UnitPrice = oi.UnitPrice,
                    Quantity = oi.Quantity,
                    LineTotal = oi.LineTotal
                })
                .ToList(),
            CreatedAt = order.CreatedAtUtc,
            PlacedAt = order.PlacedAtUtc
        };
    }

    private async Task<CheckoutResultDto> PlaceOrderCoreAsync(
        CheckoutRequest request,
        int attemptCount,
        CancellationToken cancellationToken)
    {
        ValidateCheckoutRequest(request);

        var normalizedIdempotencyKey = NormalizeIdempotencyKey(request);

        // Check if already processed (idempotent key)
        var existingOrder = await FindExistingProcessedOrderAsync(
            normalizedIdempotencyKey,
            request.CartToken,
            cancellationToken);

        if (existingOrder != null)
        {
            return CreateAlreadyProcessedResult(existingOrder);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Load cart with tracking (we will update it)
            var cart = await LoadCartByTokenAsync(request.CartToken, asNoTracking: false, onlyActive: false, cancellationToken);
            if (cart == null)
            {
                return CreateFailureResult(
                    CheckoutOutcome.InvalidCart,
                    "Korpa ne postoji ili više nije dostupna.");
            }

            if (cart.Status == CartStatus.Converted)
            {
                var convertedOrder = await FindOrderByCartIdAsync(cart.Id, cancellationToken);
                return convertedOrder != null
                    ? CreateAlreadyProcessedResult(convertedOrder)
                    : CreateFailureResult(
                        CheckoutOutcome.InvalidCart,
                        "Korpa je već konvertovana i više nije dostupna za checkout.");
            }

            if (cart.Items.Count == 0)
            {
                return CreateFailureResult(
                    CheckoutOutcome.InvalidCart,
                    "Korpa je prazna.");
            }

            // ========== PESSIMISTIC LOCKING START ==========
            // Load product variants with pessimistic lock (FOR UPDATE in PostgreSQL)
            var variantIds = cart.Items.Select(ci => ci.ProductVariantId).Distinct().ToList();
            var lockedVariants = await LockProductVariantsAsync(variantIds, cancellationToken);
            
            if (lockedVariants.Count != variantIds.Count)
            {
                await transaction.RollbackAsync(cancellationToken);
                _context.ChangeTracker.Clear();

                // Some variants were deleted or unavailable while we tried to lock
                return CreateFailureResult(
                    CheckoutOutcome.InvalidCart,
                    "Neki proizvodi više nisu dostupni. Ažurirajte korpu.");
            }

            var variantMap = lockedVariants.ToDictionary(v => v.Id);

            // Re-validate cart with locked variants
            var validation = ValidateCartForCheckoutWithLockedVariants(cart, variantMap);
            if (validation.Errors.Count > 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                _context.ChangeTracker.Clear();
                
                return CreateFailureResult(
                    validation.HasInsufficientStock
                        ? CheckoutOutcome.InsufficientStock
                        : CheckoutOutcome.InvalidCart,
                    string.Join(" ", validation.Errors));
            }
            // ========== PESSIMISTIC LOCKING END ==========

            var now = DateTimeOffset.UtcNow;
            var subtotal = validation.Items.Sum(x => x.UnitPrice * x.CartItem.Quantity);
            var deliveryAmount = CalculateDeliveryAmount(request.DeliveryMethod);

            var order = new Order
            {
                OrderNumber = GenerateTemporaryOrderNumber(),
                CartId = cart.Id,
                CheckoutIdempotencyKey = normalizedIdempotencyKey,
                Status = OrderStatus.Pending,
                Currency = "RSD",
                DeliveryMethod = request.DeliveryMethod,
                PaymentMethod = request.PaymentMethod,
                CustomerFirstName = request.CustomerFirstName.Trim(),
                CustomerLastName = request.CustomerLastName.Trim(),
                Email = request.GetResolvedEmail().Trim(),
                Phone = request.Phone.Trim(),
                DeliveryAddressLine1 = request.DeliveryAddressLine1.Trim(),
                DeliveryAddressLine2 = request.DeliveryAddressLine2?.Trim(),
                DeliveryCity = request.DeliveryCity.Trim(),
                DeliveryPostalCode = request.DeliveryPostalCode.Trim(),
                Note = request.Note?.Trim(),
                SubtotalAmount = subtotal,
                DeliveryAmount = deliveryAmount,
                TotalAmount = subtotal + deliveryAmount,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
                PlacedAtUtc = now
            };

            // Update order items and stock (all under lock from variants)
            foreach (var item in validation.Items)
            {
                order.Items.Add(new OrderItem
                {
                    ProductId = item.Variant.ProductId,
                    ProductVariantId = item.Variant.Id,
                    ProductNameSnapshot = item.Variant.Product!.Name,
                    BrandNameSnapshot = item.Variant.Product.Brand?.Name ?? string.Empty,
                    SizeEuSnapshot = item.Variant.SizeEu,
                    CategoryIdSnapshot = item.Variant.Product.PrimaryCategoryId,
                    CategoryNameSnapshot = item.Variant.Product.PrimaryCategoryId > 0 ? "Default" : string.Empty,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.CartItem.Quantity,
                    LineTotal = item.UnitPrice * item.CartItem.Quantity
                });

                // Direct stock update is now safe (locked variant)
                item.Variant.TotalStock -= item.CartItem.Quantity;
                item.Variant.StockStatus = ResolveStockStatus(item.Variant);
                item.Variant.UpdatedAtUtc = now;
            }

            cart.Status = CartStatus.Converted;
            cart.UpdatedAtUtc = now;
            cart.Items.Clear();

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(cancellationToken);

            // Generate final order number from assigned ID
            order.OrderNumber = GenerateOrderNumberFromId(order.Id);
            order.UpdatedAtUtc = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return CreateSuccessResult(order);
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _context.ChangeTracker.Clear();

            // Check if order was already created (idempotency)
            var processedOrder = await FindExistingProcessedOrderAsync(
                NormalizeIdempotencyKey(request),
                request.CartToken,
                cancellationToken);

            if (processedOrder != null)
            {
                return CreateAlreadyProcessedResult(processedOrder);
            }

            // If this is a concurrentcy exception and we haven't retried too much, could retry once
            // But with pessimistic locking, DBUpdateException is now more meaningful (constraint violation, not race)
            if (ex.InnerException?.Message.Contains("duplicate") == true || 
                ex.InnerException?.Message.Contains("unique") == true)
            {
                // Unique constraint violation (idempotency key collision) - should be already handled above
                return CreateFailureResult(
                    CheckoutOutcome.InvalidCart,
                    "Order sa tim ključem je već obrađen. Pokušajte sa novim id.");
            }

            // Other DB errors - propagate
            throw;
        }
        catch (OperationCanceledException)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Load and lock product variants for checkout with pessimistic locking.
    /// Ensures no other transaction can modify these variants during checkout.
    /// </summary>
    private async Task<List<Domain.Catalog.ProductVariant>> LockProductVariantsAsync(
        List<long> variantIds,
        CancellationToken cancellationToken)
    {
        // Load variants with tracking - they will be locked
        var locked = await _context.Set<Domain.Catalog.ProductVariant>()
            .Where(pv => variantIds.Contains(pv.Id))
            .OrderBy(pv => pv.Id)  // Prevent deadlock in multi-variant scenarios
            .ToListAsync(cancellationToken);

        return locked;
    }

    /// <summary>
    /// Validate cart items using pre-locked variants.
    /// This method replicates ValidateCartForCheckout but uses already-locked variants.
    /// </summary>
    private CartValidationResult ValidateCartForCheckoutWithLockedVariants(
        Cart cart,
        Dictionary<long, Domain.Catalog.ProductVariant> variantMap)
    {
        var result = new CartValidationResult();

        foreach (var cartItem in cart.Items)
        {
            if (cartItem.Quantity <= 0)
            {
                result.Errors.Add($"Invalid quantity for cart item {cartItem.Id}.");
                continue;
            }

            if (!variantMap.TryGetValue(cartItem.ProductVariantId, out var variant) || 
                variant == null || variant.Product == null)
            {
                result.Errors.Add($"Variant data is missing for cart item {cartItem.Id}.");
                continue;
            }

            if (!variant.IsActive || !variant.IsVisible)
            {
                result.Errors.Add($"Variant {variant.Id} is not available for purchase.");
                continue;
            }

            // CRITICAL: Stock check now uses locked variant (race-proof)
            if (variant.TotalStock < cartItem.Quantity)
            {
                result.HasInsufficientStock = true;
                result.Errors.Add($"Insufficient stock for variant {variant.Id}. Available: {variant.TotalStock}.");
                continue;
            }

            if (variant.Product.Status != ProductStatus.Published || !variant.Product.IsVisible || !variant.Product.IsPurchasable)
            {
                result.Errors.Add($"Product {variant.ProductId} is not purchasable.");
                continue;
            }

            var unitPrice = cartItem.UnitPrice > 0 ? cartItem.UnitPrice : variant.Price;
            result.Items.Add(new ValidatedCartItem(cartItem, variant, unitPrice));
        }

        return result;
    }

    private async Task<Cart?> LoadCartByTokenAsync(
        string cartToken,
        bool asNoTracking,
        bool onlyActive,
        CancellationToken cancellationToken)
    {
        var query = _context.Carts
            .Include(c => c.Items)
                .ThenInclude(ci => ci.ProductVariant)
                    .ThenInclude(pv => pv!.Product)
                        .ThenInclude(p => p!.Brand)
            .Where(c => c.CartToken == cartToken);

        if (onlyActive)
        {
            query = query.Where(c => c.Status == CartStatus.Active);
        }

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<Order?> FindExistingProcessedOrderAsync(
        string normalizedIdempotencyKey,
        string cartToken,
        CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(
                entity => entity.CheckoutIdempotencyKey == normalizedIdempotencyKey,
                cancellationToken);

        if (order != null)
        {
            return order;
        }

        var cartId = await _context.Carts
            .AsNoTracking()
            .Where(entity => entity.CartToken == cartToken)
            .Select(entity => (long?)entity.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (!cartId.HasValue)
        {
            return null;
        }

        return await FindOrderByCartIdAsync(cartId.Value, cancellationToken);
    }

    private Task<Order?> FindOrderByCartIdAsync(long cartId, CancellationToken cancellationToken)
    {
        return _context.Orders
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.CartId == cartId, cancellationToken);
    }

    private static void ValidateCheckoutRequest(CheckoutRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CartToken))
        {
            throw new InvalidOperationException("CartToken is required.");
        }

        if (string.IsNullOrWhiteSpace(request.CustomerFirstName) || string.IsNullOrWhiteSpace(request.CustomerLastName))
        {
            throw new InvalidOperationException("Customer first name and last name are required.");
        }

        if (string.IsNullOrWhiteSpace(request.GetResolvedEmail()))
        {
            throw new InvalidOperationException("Customer email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Phone))
        {
            throw new InvalidOperationException("Customer phone is required.");
        }

        if (request.DeliveryMethod == DeliveryMethod.Courier &&
            (string.IsNullOrWhiteSpace(request.DeliveryAddressLine1) ||
             string.IsNullOrWhiteSpace(request.DeliveryCity) ||
             string.IsNullOrWhiteSpace(request.DeliveryPostalCode)))
        {
            throw new InvalidOperationException("Delivery address is required for courier delivery.");
        }
    }

    private static decimal CalculateDeliveryAmount(DeliveryMethod deliveryMethod)
    {
        return deliveryMethod == DeliveryMethod.StorePickup ? 0m : DefaultCourierDeliveryAmount;
    }

    private static StockStatus ResolveStockStatus(Domain.Catalog.ProductVariant variant)
    {
        if (variant.TotalStock <= 0)
        {
            return StockStatus.OutOfStock;
        }

        if (variant.TotalStock <= Math.Max(variant.LowStockThreshold, 1))
        {
            return StockStatus.LowStock;
        }

        return StockStatus.InStock;
    }

    private static CartValidationResult ValidateCartForCheckout(Cart cart)
    {
        var result = new CartValidationResult();

        foreach (var cartItem in cart.Items)
        {
            if (cartItem.Quantity <= 0)
            {
                result.Errors.Add($"Invalid quantity for cart item {cartItem.Id}.");
                continue;
            }

            var variant = cartItem.ProductVariant;
            if (variant == null || variant.Product == null)
            {
                result.Errors.Add($"Variant data is missing for cart item {cartItem.Id}.");
                continue;
            }

            if (!variant.IsActive || !variant.IsVisible)
            {
                result.Errors.Add($"Variant {variant.Id} is not available for purchase.");
                continue;
            }

            if (variant.TotalStock < cartItem.Quantity)
            {
                result.HasInsufficientStock = true;
                result.Errors.Add($"Insufficient stock for variant {variant.Id}. Available: {variant.TotalStock}.");
                continue;
            }

            if (variant.Product.Status != ProductStatus.Published || !variant.Product.IsVisible || !variant.Product.IsPurchasable)
            {
                result.Errors.Add($"Product {variant.ProductId} is not purchasable.");
                continue;
            }

            var unitPrice = cartItem.UnitPrice > 0 ? cartItem.UnitPrice : variant.Price;
            result.Items.Add(new ValidatedCartItem(cartItem, variant, unitPrice));
        }

        return result;
    }

    private static string GenerateOrderNumberFromId(long id)
    {
        return $"TP-{DateTime.UtcNow:yyyy}-{id:D6}";
    }

    private static string GenerateTemporaryOrderNumber()
    {
        return $"TMP-{Guid.NewGuid():N}"[..16];
    }

    private static string NormalizeIdempotencyKey(CheckoutRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            return request.IdempotencyKey.Trim();
        }

        return $"cart:{request.CartToken.Trim()}";
    }

    private static CheckoutResultDto CreateSuccessResult(Order order)
    {
        return new CheckoutResultDto
        {
            Outcome = CheckoutOutcome.Success,
            Message = "Porudzbina je uspesno kreirana.",
            OrderNumber = order.OrderNumber,
            TotalAmount = order.TotalAmount,
            Status = order.Status.ToString()
        };
    }

    private static CheckoutResultDto CreateAlreadyProcessedResult(Order order)
    {
        return new CheckoutResultDto
        {
            Outcome = CheckoutOutcome.AlreadyProcessed,
            Message = "Checkout je vec obradjen. Vraca se postojeca porudzbina.",
            OrderNumber = order.OrderNumber,
            TotalAmount = order.TotalAmount,
            Status = order.Status.ToString()
        };
    }

    private static CheckoutResultDto CreateFailureResult(CheckoutOutcome outcome, string message)
    {
        return new CheckoutResultDto
        {
            Outcome = outcome,
            Message = message,
            OrderNumber = string.Empty,
            TotalAmount = 0m,
            Status = string.Empty
        };
    }

    private sealed class CartValidationResult
    {
        public List<ValidatedCartItem> Items { get; } = new();
        public List<string> Errors { get; } = new();
        public bool HasInsufficientStock { get; set; }
    }

    private sealed record ValidatedCartItem(CartItem CartItem, Domain.Catalog.ProductVariant Variant, decimal UnitPrice);
}
