using Microsoft.EntityFrameworkCore;
using TrendplusProdavnica.Application.Checkout.Dtos;
using TrendplusProdavnica.Domain.Sales;
using TrendplusProdavnica.Infrastructure.Persistence;

namespace TrendplusProdavnica.Infrastructure.Services;

public interface ICheckoutService
{
    Task<CheckoutSummaryDto?> GetCheckoutSummaryAsync(string cartToken, CancellationToken cancellationToken = default);
    Task<CheckoutResultDto> PlaceOrderAsync(CheckoutRequest request, CancellationToken cancellationToken = default);
    Task<OrderDto?> GetOrderByNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
}

public class CheckoutService : ICheckoutService
{
    private readonly TrendplusDbContext _context;

    public CheckoutService(TrendplusDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene el resumen del carrito para el checkout
    /// </summary>
    public async Task<CheckoutSummaryDto?> GetCheckoutSummaryAsync(string cartToken, CancellationToken cancellationToken = default)
    {
        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(ci => ci.ProductVariant)
            .ThenInclude(pv => pv.Product)
            .ThenInclude(p => p.Brand)
            .FirstOrDefaultAsync(c => c.CartToken == cartToken, cancellationToken);

        if (cart == null || cart.Items.Count == 0)
            return null;

        var items = cart.Items
            .Where(ci => ci.ProductVariant != null && ci.ProductVariant.IsActive && ci.ProductVariant.IsVisible)
            .Select(ci => new CartItemSummaryDto
            {
                ProductName = ci.ProductVariant!.Product.Name,
                BrandName = ci.ProductVariant.Product.Brand.Name,
                SizeEu = ci.ProductVariant.SizeEu,
                UnitPrice = ci.ProductVariant.Price,
                Quantity = ci.Quantity,
                LineTotal = ci.ProductVariant.Price * ci.Quantity,
            })
            .ToList();

        if (items.Count == 0)
            return null;

        var subtotal = items.Sum(i => i.LineTotal);
        // NOTE: delivery fee is currently hard-coded here.
        // PROBLEM: This does not take the customer's selected delivery method into account
        // (e.g. StorePickup should be free). `PlaceOrderAsync` copies this summary value
        // into the saved order, so store-pickup orders may be incorrectly charged shipping.
        // SUGGESTION: Pass the selected `DeliveryMethod` into `GetCheckoutSummaryAsync`
        // or compute delivery amount in `PlaceOrderAsync` from `request.DeliveryMethod`.
        // Alternatively, consult a delivery-pricing service or table here instead
        // of using a fixed value.
        var deliveryAmount = 300m; // V1: default delivery cost (Can be extended)
        var total = subtotal + deliveryAmount;

        return new CheckoutSummaryDto
        {
            CartToken = cartToken,
            Items = items,
            SubtotalAmount = subtotal,
            DeliveryAmount = deliveryAmount,
            TotalAmount = total,
            ItemCount = items.Sum(i => i.Quantity),
        };
    }

    /// <summary>
    /// Crea una orden a partir del carrito
    /// </summary>
    public async Task<CheckoutResultDto> PlaceOrderAsync(CheckoutRequest request, CancellationToken cancellationToken = default)
    {
        // 1. Cargar el carrito
        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(ci => ci.ProductVariant)
            .ThenInclude(pv => pv.Product)
            .ThenInclude(p => p.Brand)
            .FirstOrDefaultAsync(c => c.CartToken == request.CartToken, cancellationToken);

        if (cart == null || cart.Items.Count == 0)
            throw new InvalidOperationException("El carrito no existe o está vacío");

        // 2. Validar y obtener resumen
        var summary = await GetCheckoutSummaryAsync(request.CartToken, cancellationToken);
        if (summary == null)
            throw new InvalidOperationException("No se puede procesar el carrito");

        // Compute delivery amount according to requested delivery method (avoid charging store pickup)
        var deliveryAmount = request.DeliveryMethod == DeliveryMethod.StorePickup ? 0m : 300m;
        summary.DeliveryAmount = deliveryAmount;
        summary.TotalAmount = summary.SubtotalAmount + deliveryAmount;

        // 3. Crear la orden (OrderNumber will be assigned after saving to use DB Id for deterministic sequence)
        var order = new Order
        {
            // OrderNumber will be set after SaveChanges to use the DB-generated Id
            CartId = cart.Id,
            Status = OrderStatus.PendingPayment,
            Currency = "RSD",
            DeliveryMethod = request.DeliveryMethod,
            PaymentMethod = request.PaymentMethod,
            
            CustomerFirstName = request.CustomerFirstName,
            CustomerLastName = request.CustomerLastName,
            Email = request.Email,
            Phone = request.Phone,
            
            DeliveryAddressLine1 = request.DeliveryAddressLine1,
            DeliveryAddressLine2 = request.DeliveryAddressLine2,
            DeliveryCity = request.DeliveryCity,
            DeliveryPostalCode = request.DeliveryPostalCode,
            
            Note = request.Note,
            
            SubtotalAmount = summary.SubtotalAmount,
            DeliveryAmount = summary.DeliveryAmount,
            TotalAmount = summary.TotalAmount,
            
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            PlacedAtUtc = DateTimeOffset.UtcNow,
        };

        // 5. Agregar items a la orden
        foreach (var cartItem in summary.Items)
        {
            var dbCartItem = cart.Items.First(ci =>
                ci.ProductVariant!.Product.Name == cartItem.ProductName &&
                ci.ProductVariant.SizeEu == cartItem.SizeEu);

            order.Items.Add(new OrderItem
            {
                ProductId = dbCartItem.ProductVariantId,
                ProductVariantId = dbCartItem.ProductVariantId,
                ProductNameSnapshot = cartItem.ProductName,
                BrandNameSnapshot = cartItem.BrandName,
                SizeEuSnapshot = cartItem.SizeEu,
                UnitPrice = cartItem.UnitPrice,
                Quantity = cartItem.Quantity,
                LineTotal = cartItem.LineTotal,
            });
        }

        // 6. Guardar en la base de datos
        _context.Orders.Add(order);
        // Save first to obtain DB-generated Id (used to generate a deterministic order number)
        await _context.SaveChangesAsync(cancellationToken);

        // Generate order number based on DB id to avoid race conditions
        order.OrderNumber = GenerateOrderNumberFromId(order.Id);
        await _context.SaveChangesAsync(cancellationToken);

        return new CheckoutResultDto
        {
            OrderNumber = order.OrderNumber,
            TotalAmount = order.TotalAmount,
            Status = order.Status.ToString(),
        };
    }

    /// <summary>
    /// Obtiene una orden por número
    /// </summary>
    public async Task<OrderDto?> GetOrderByNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);

        if (order == null)
            return null;

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
                    LineTotal = oi.LineTotal,
                })
                .ToList(),
                
            CreatedAt = order.CreatedAtUtc,
            PlacedAt = order.PlacedAtUtc,
        };
    }

    /// <summary>
    /// Genera un número de orden único
    /// Formato: TP-2026-000001
    /// </summary>
    private string GenerateOrderNumberFromId(long id)
    {
        var year = DateTime.UtcNow.Year;
        return $"TP-{year}-{id:D6}";
    }
}
