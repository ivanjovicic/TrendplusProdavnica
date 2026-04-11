#nullable enable
using Microsoft.EntityFrameworkCore;
using TrendplusProdavnica.Application.Cart.Dtos;
using TrendplusProdavnica.Application.Cart.Services;
using TrendplusProdavnica.Domain.Catalog;
using TrendplusProdavnica.Domain.Enums;
using TrendplusProdavnica.Domain.Sales;
using TrendplusProdavnica.Infrastructure.Persistence;

namespace TrendplusProdavnica.Infrastructure.Services
{
    /// <summary>
    /// Shopping cart service implementation using EF Core.
    /// </summary>
    public class CartService : ICartService
    {
        private const int MaxQuantityPerVariant = 10;
        private readonly TrendplusDbContext _dbContext;

        public CartService(TrendplusDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CartDto> CreateCartAsync(CancellationToken cancellationToken = default)
        {
            var cart = new Cart
            {
                CartToken = Guid.NewGuid().ToString("N"),
                Status = CartStatus.Active,
                Currency = "RSD"
            };

            _dbContext.Carts.Add(cart);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return MapToDto(cart);
        }

        public async Task<CartDto?> GetCartAsync(string cartToken, CancellationToken cancellationToken = default)
        {
            var cart = await _dbContext.Carts
                .AsNoTracking()
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.CartToken == cartToken && x.Status == CartStatus.Active, cancellationToken);

            if (cart == null)
            {
                return null;
            }

            return await MapToDtoWithDetailsAsync(cart, cancellationToken);
        }

        public async Task<CartDto?> GetCartBySessionAsync(string sessionId, string? userId = null, CancellationToken cancellationToken = default)
        {
            ValidateSessionId(sessionId);

            var cartQuery = _dbContext.Carts
                .AsNoTracking()
                .Include(x => x.Items)
                .Where(x => x.Status == CartStatus.Active && x.SessionId == sessionId);

            if (!string.IsNullOrWhiteSpace(userId))
            {
                cartQuery = cartQuery.Where(x => x.UserId == userId);
            }

            var cart = await cartQuery
                .OrderByDescending(x => x.UpdatedAtUtc)
                .ThenByDescending(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (cart == null)
            {
                return null;
            }

            return await MapToDtoWithDetailsAsync(cart, cancellationToken);
        }

        public async Task<CartDto> AddItemAsync(string cartToken, AddToCartRequest request, CancellationToken cancellationToken = default)
        {
            if (request.Quantity <= 0)
            {
                throw new ArgumentException("Quantity must be greater than 0", nameof(request.Quantity));
            }

            var cart = await _dbContext.Carts
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.CartToken == cartToken && x.Status == CartStatus.Active, cancellationToken)
                ?? throw new KeyNotFoundException($"Cart {cartToken} not found");

            await AddOrIncreaseItemAsync(cart, request.ProductVariantId, request.Quantity, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return await MapToDtoWithDetailsAsync(cart, cancellationToken);
        }

        public async Task<CartDto> AddItemBySessionAsync(AddToCartBySessionRequest request, CancellationToken cancellationToken = default)
        {
            ValidateSessionId(request.SessionId);

            if (request.Quantity <= 0)
            {
                throw new ArgumentException("Quantity must be greater than 0", nameof(request.Quantity));
            }

            var cart = await GetOrCreateActiveCartBySessionAsync(request.SessionId, request.UserId, cancellationToken);

            await AddOrIncreaseItemAsync(cart, request.ProductVariantId, request.Quantity, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return await MapToDtoWithDetailsAsync(cart, cancellationToken);
        }

        public async Task<CartDto> UpdateItemQuantityAsync(string cartToken, long itemId, UpdateCartItemRequest request, CancellationToken cancellationToken = default)
        {
            if (request.Quantity <= 0)
            {
                throw new ArgumentException("Quantity must be greater than 0", nameof(request.Quantity));
            }

            var cart = await _dbContext.Carts
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.CartToken == cartToken && x.Status == CartStatus.Active, cancellationToken)
                ?? throw new KeyNotFoundException($"Cart {cartToken} not found");

            var item = cart.Items.FirstOrDefault(x => x.Id == itemId)
                ?? throw new KeyNotFoundException($"Cart item {itemId} not found");

            await SetItemQuantityAsync(cart, item, request.Quantity, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return await MapToDtoWithDetailsAsync(cart, cancellationToken);
        }

        public async Task<CartDto> UpdateItemBySessionAsync(UpdateCartBySessionRequest request, CancellationToken cancellationToken = default)
        {
            ValidateSessionId(request.SessionId);

            if (request.Quantity <= 0)
            {
                throw new ArgumentException("Quantity must be greater than 0", nameof(request.Quantity));
            }

            var cartQuery = _dbContext.Carts
                .Include(x => x.Items)
                .Where(x => x.Status == CartStatus.Active && x.SessionId == request.SessionId);

            if (!string.IsNullOrWhiteSpace(request.UserId))
            {
                cartQuery = cartQuery.Where(x => x.UserId == request.UserId);
            }

            var cart = await cartQuery
                .OrderByDescending(x => x.UpdatedAtUtc)
                .ThenByDescending(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new KeyNotFoundException($"Active cart for session {request.SessionId} not found");

            var item = cart.Items.FirstOrDefault(x => x.ProductVariantId == request.ProductVariantId)
                ?? throw new KeyNotFoundException($"Cart item for variant {request.ProductVariantId} not found");

            await SetItemQuantityAsync(cart, item, request.Quantity, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return await MapToDtoWithDetailsAsync(cart, cancellationToken);
        }

        public async Task<CartDto> RemoveItemAsync(string cartToken, long itemId, CancellationToken cancellationToken = default)
        {
            var cart = await _dbContext.Carts
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.CartToken == cartToken && x.Status == CartStatus.Active, cancellationToken)
                ?? throw new KeyNotFoundException($"Cart {cartToken} not found");

            var item = cart.Items.FirstOrDefault(x => x.Id == itemId)
                ?? throw new KeyNotFoundException($"Cart item {itemId} not found");

            cart.Items.Remove(item);
            cart.UpdatedAtUtc = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return await MapToDtoWithDetailsAsync(cart, cancellationToken);
        }

        public async Task<CartDto> RemoveItemBySessionAsync(RemoveFromCartRequest request, CancellationToken cancellationToken = default)
        {
            ValidateSessionId(request.SessionId);

            var cartQuery = _dbContext.Carts
                .Include(x => x.Items)
                .Where(x => x.Status == CartStatus.Active && x.SessionId == request.SessionId);

            if (!string.IsNullOrWhiteSpace(request.UserId))
            {
                cartQuery = cartQuery.Where(x => x.UserId == request.UserId);
            }

            var cart = await cartQuery
                .OrderByDescending(x => x.UpdatedAtUtc)
                .ThenByDescending(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new KeyNotFoundException($"Active cart for session {request.SessionId} not found");

            var item = cart.Items.FirstOrDefault(x => x.ProductVariantId == request.ProductVariantId)
                ?? throw new KeyNotFoundException($"Cart item for variant {request.ProductVariantId} not found");

            cart.Items.Remove(item);
            cart.UpdatedAtUtc = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return await MapToDtoWithDetailsAsync(cart, cancellationToken);
        }

        public async Task<CartDto> ClearCartAsync(string cartToken, CancellationToken cancellationToken = default)
        {
            var cart = await _dbContext.Carts
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.CartToken == cartToken && x.Status == CartStatus.Active, cancellationToken)
                ?? throw new KeyNotFoundException($"Cart {cartToken} not found");

            cart.Items.Clear();
            cart.UpdatedAtUtc = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return MapToDto(cart);
        }

        private async Task<Cart> GetOrCreateActiveCartBySessionAsync(string sessionId, string? userId, CancellationToken cancellationToken)
        {
            var cartQuery = _dbContext.Carts
                .Include(x => x.Items)
                .Where(x => x.Status == CartStatus.Active && x.SessionId == sessionId);

            if (!string.IsNullOrWhiteSpace(userId))
            {
                cartQuery = cartQuery.Where(x => x.UserId == userId);
            }

            var existingCart = await cartQuery
                .OrderByDescending(x => x.UpdatedAtUtc)
                .ThenByDescending(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingCart != null)
            {
                return existingCart;
            }

            var cart = new Cart
            {
                CartToken = Guid.NewGuid().ToString("N"),
                SessionId = sessionId,
                UserId = userId,
                Status = CartStatus.Active,
                Currency = "RSD"
            };

            _dbContext.Carts.Add(cart);
            return cart;
        }

        private async Task AddOrIncreaseItemAsync(Cart cart, long productVariantId, int quantityToAdd, CancellationToken cancellationToken)
        {
            var variant = await GetPurchasableVariantAsync(productVariantId, cancellationToken);
            var existingItem = cart.Items.FirstOrDefault(x => x.ProductVariantId == productVariantId);
            var requestedQuantity = (existingItem?.Quantity ?? 0) + quantityToAdd;

            ValidateRequestedQuantity(variant, requestedQuantity);

            if (existingItem != null)
            {
                existingItem.Quantity = requestedQuantity;
                existingItem.UpdatedAtUtc = DateTimeOffset.UtcNow;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ProductVariantId = productVariantId,
                    Quantity = requestedQuantity,
                    UnitPrice = variant.Price
                });
            }

            cart.UpdatedAtUtc = DateTimeOffset.UtcNow;
        }

        private async Task SetItemQuantityAsync(Cart cart, CartItem item, int quantity, CancellationToken cancellationToken)
        {
            var variant = await GetPurchasableVariantAsync(item.ProductVariantId, cancellationToken);
            ValidateRequestedQuantity(variant, quantity);

            item.Quantity = quantity;
            item.UpdatedAtUtc = DateTimeOffset.UtcNow;
            cart.UpdatedAtUtc = DateTimeOffset.UtcNow;
        }

        private async Task<ProductVariant> GetPurchasableVariantAsync(long productVariantId, CancellationToken cancellationToken)
        {
            var variant = await _dbContext.ProductVariants
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == productVariantId, cancellationToken)
                ?? throw new KeyNotFoundException($"Product variant {productVariantId} not found");

            if (!variant.IsActive || !variant.IsVisible)
            {
                throw new InvalidOperationException("Product variant is not available for purchase");
            }

            return variant;
        }

        private static void ValidateRequestedQuantity(ProductVariant variant, int requestedQuantity)
        {
            if (requestedQuantity <= 0)
            {
                throw new InvalidOperationException("Quantity must be greater than 0");
            }

            var maxAllowedQuantity = Math.Min(MaxQuantityPerVariant, variant.TotalStock);
            if (maxAllowedQuantity <= 0)
            {
                throw new InvalidOperationException("Product variant is out of stock");
            }

            if (requestedQuantity > maxAllowedQuantity)
            {
                throw new InvalidOperationException($"Maximum quantity for this variant is {maxAllowedQuantity}");
            }
        }

        private static void ValidateSessionId(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                throw new ArgumentException("SessionId is required", nameof(sessionId));
            }
        }

        private static CartDto MapToDto(Cart cart)
        {
            return new CartDto(
                CartId: cart.Id,
                UserId: cart.UserId,
                SessionId: cart.SessionId,
                CartToken: cart.CartToken,
                Currency: cart.Currency,
                Items: Array.Empty<CartItemDto>(),
                TotalItems: 0,
                TotalAmount: 0m);
        }

        private async Task<CartDto> MapToDtoWithDetailsAsync(Cart cart, CancellationToken cancellationToken)
        {
            var cartItems = new List<CartItemDto>();
            decimal totalAmount = 0m;

            var variantIds = cart.Items.Select(x => x.ProductVariantId).Distinct().ToArray();
            if (variantIds.Length > 0)
            {
                var variants = await _dbContext.ProductVariants
                    .AsNoTracking()
                    .Where(x => variantIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id, cancellationToken);

                var productIds = variants.Values.Select(x => x.ProductId).Distinct().ToArray();

                var productDetails = await _dbContext.Products
                    .AsNoTracking()
                    .Where(x => productIds.Contains(x.Id))
                    .Select(product => new
                    {
                        product.Id,
                        product.Slug,
                        product.Name,
                        BrandName = product.Brand != null ? product.Brand.Name : string.Empty,
                        PrimaryImageUrl = product.Media
                            .Where(media => media.IsActive && media.IsPrimary)
                            .OrderBy(media => media.SortOrder)
                            .Select(media => media.Url)
                            .FirstOrDefault() ??
                            product.Media
                                .Where(media => media.IsActive)
                                .OrderBy(media => media.SortOrder)
                                .Select(media => media.Url)
                                .FirstOrDefault()
                    })
                    .ToDictionaryAsync(x => x.Id, cancellationToken);

                foreach (var item in cart.Items)
                {
                    if (!variants.TryGetValue(item.ProductVariantId, out var variant))
                    {
                        continue;
                    }

                    if (!productDetails.TryGetValue(variant.ProductId, out var product))
                    {
                        continue;
                    }

                    var lineTotal = item.UnitPrice * item.Quantity;
                    totalAmount += lineTotal;

                    cartItems.Add(new CartItemDto(
                        ItemId: item.Id,
                        ProductVariantId: item.ProductVariantId,
                        ProductSlug: product.Slug,
                        ProductName: product.Name,
                        BrandName: product.BrandName,
                        SizeEu: variant.SizeEu,
                        PrimaryImageUrl: product.PrimaryImageUrl,
                        UnitPrice: item.UnitPrice,
                        Quantity: item.Quantity,
                        LineTotal: lineTotal,
                        IsAvailable: variant.IsActive && variant.IsVisible && variant.TotalStock > 0,
                        IsLowStock: variant.TotalStock > 0 && variant.TotalStock <= Math.Max(variant.LowStockThreshold, 1)));
                }
            }

            return new CartDto(
                CartId: cart.Id,
                UserId: cart.UserId,
                SessionId: cart.SessionId,
                CartToken: cart.CartToken,
                Currency: cart.Currency,
                Items: cartItems,
                TotalItems: cart.Items.Sum(x => x.Quantity),
                TotalAmount: totalAmount);
        }
    }
}
