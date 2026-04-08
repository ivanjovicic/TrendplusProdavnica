#nullable enable
using TrendplusProdavnica.Application.Cart.Dtos;
using TrendplusProdavnica.Application.Cart.Services;
using TrendplusProdavnica.Domain.Sales;
using TrendplusProdavnica.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace TrendplusProdavnica.Infrastructure.Services
{
    /// <summary>
    /// Shopping cart service implementation using EF Core
    /// </summary>
    public class CartService : ICartService
    {
        private readonly TrendplusDbContext _dbContext;

        public CartService(TrendplusDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Create a new empty cart with unique token
        /// </summary>
        public async Task<CartDto> CreateCartAsync(CancellationToken cancellationToken = default)
        {
            var cart = new Cart
            {
                CartToken = Guid.NewGuid().ToString(),
                Status = Domain.Enums.CartStatus.Active,
                Currency = "RSD"
            };

            _dbContext.Carts.Add(cart);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return MapToDto(cart);
        }

        /// <summary>
        /// Get cart with all items and product details
        /// </summary>
        public async Task<CartDto?> GetCartAsync(string cartToken, CancellationToken cancellationToken = default)
        {
            var cart = await _dbContext.Carts
                .AsNoTracking()
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.CartToken == cartToken, cancellationToken);

            if (cart == null)
                return null;

            return await MapToDtoWithDetailsAsync(cart, cancellationToken);
        }

        /// <summary>
        /// Add item to cart or increase quantity if variant exists
        /// </summary>
        public async Task<CartDto> AddItemAsync(string cartToken, AddToCartRequest request, CancellationToken cancellationToken = default)
        {
            var cart = await _dbContext.Carts
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.CartToken == cartToken, cancellationToken)
                ?? throw new KeyNotFoundException($"Cart {cartToken} not found");

            // Validate variant exists and is available
            var variant = await _dbContext.ProductVariants
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.ProductVariantId, cancellationToken)
                ?? throw new KeyNotFoundException($"Product variant {request.ProductVariantId} not found");

            if (!variant.IsActive || !variant.IsVisible)
                throw new InvalidOperationException("Product variant is not available for purchase");

            if (request.Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than 0", nameof(request.Quantity));

            // Check if item already exists in cart
            var existingItem = cart.Items.FirstOrDefault(x => x.ProductVariantId == request.ProductVariantId);

            if (existingItem != null)
            {
                // Increase quantity
                existingItem.Quantity += request.Quantity;
                existingItem.UpdatedAtUtc = DateTimeOffset.UtcNow;
            }
            else
            {
                // Add new item
                var item = new CartItem
                {
                    CartId = cart.Id,
                    ProductVariantId = request.ProductVariantId,
                    Quantity = request.Quantity,
                    UnitPrice = variant.Price
                };
                cart.Items.Add(item);
            }

            cart.UpdatedAtUtc = DateTimeOffset.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return await MapToDtoWithDetailsAsync(cart, cancellationToken);
        }

        /// <summary>
        /// Update item quantity
        /// </summary>
        public async Task<CartDto> UpdateItemQuantityAsync(string cartToken, long itemId, UpdateCartItemRequest request, CancellationToken cancellationToken = default)
        {
            var cart = await _dbContext.Carts
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.CartToken == cartToken, cancellationToken)
                ?? throw new KeyNotFoundException($"Cart {cartToken} not found");

            if (request.Quantity <= 0)
                throw new ArgumentException("Quantity must be greater than 0", nameof(request.Quantity));

            var item = cart.Items.FirstOrDefault(x => x.Id == itemId)
                ?? throw new KeyNotFoundException($"Cart item {itemId} not found");

            item.Quantity = request.Quantity;
            item.UpdatedAtUtc = DateTimeOffset.UtcNow;
            cart.UpdatedAtUtc = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return await MapToDtoWithDetailsAsync(cart, cancellationToken);
        }

        /// <summary>
        /// Remove single item from cart
        /// </summary>
        public async Task<CartDto> RemoveItemAsync(string cartToken, long itemId, CancellationToken cancellationToken = default)
        {
            var cart = await _dbContext.Carts
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.CartToken == cartToken, cancellationToken)
                ?? throw new KeyNotFoundException($"Cart {cartToken} not found");

            var item = cart.Items.FirstOrDefault(x => x.Id == itemId)
                ?? throw new KeyNotFoundException($"Cart item {itemId} not found");

            cart.Items.Remove(item);
            cart.UpdatedAtUtc = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return await MapToDtoWithDetailsAsync(cart, cancellationToken);
        }

        /// <summary>
        /// Clear all items from cart
        /// </summary>
        public async Task<CartDto> ClearCartAsync(string cartToken, CancellationToken cancellationToken = default)
        {
            var cart = await _dbContext.Carts
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.CartToken == cartToken, cancellationToken)
                ?? throw new KeyNotFoundException($"Cart {cartToken} not found");

            cart.Items.Clear();
            cart.UpdatedAtUtc = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return MapToDto(cart);
        }

        /// <summary>
        /// Map Cart domain entity to DTO (empty cart)
        /// </summary>
        private CartDto MapToDto(Cart cart)
        {
            return new CartDto(
                CartToken: cart.CartToken,
                Currency: cart.Currency,
                Items: Array.Empty<CartItemDto>(),
                TotalItems: 0,
                TotalAmount: 0m
            );
        }

        /// <summary>
        /// Map Cart domain entity to DTO with full item and product details
        /// </summary>
        private async Task<CartDto> MapToDtoWithDetailsAsync(Cart cart, CancellationToken cancellationToken)
        {
            var cartItems = new List<CartItemDto>();
            decimal totalAmount = 0m;

            var variantIds = cart.Items.Select(x => x.ProductVariantId).ToList();
            if (variantIds.Count > 0)
            {
                // Load all variants
                var variants = await _dbContext.ProductVariants
                    .AsNoTracking()
                    .Where(x => variantIds.Contains(x.Id))
                    .ToListAsync(cancellationToken);

                // Get unique product IDs
                var productIds = variants.Select(x => x.ProductId).Distinct().ToList();

                // Load all products with media
                var products = await _dbContext.Products
                    .AsNoTracking()
                    .Where(x => productIds.Contains(x.Id))
                    .Include(x => x.Media)
                    .ToListAsync(cancellationToken);

                foreach (var item in cart.Items)
                {
                    var variant = variants.FirstOrDefault(x => x.Id == item.ProductVariantId);
                    if (variant == null)
                        continue;

                    var product = products.FirstOrDefault(x => x.Id == variant.ProductId);
                    if (product == null)
                        continue;

                    var lineTotal = item.UnitPrice * item.Quantity;
                    totalAmount += lineTotal;

                    var primaryImage = product.Media
                        .FirstOrDefault(x => x.IsPrimary)?
                        .Url;

                    var cartItem = new CartItemDto(
                        ItemId: item.Id,
                        ProductVariantId: item.ProductVariantId,
                        ProductSlug: product.Slug,
                        ProductName: product.Name,
                        BrandName: "", // TODO: Include Brand via ProductBrand relationship
                        SizeEu: variant.SizeEu,
                        PrimaryImageUrl: primaryImage,
                        UnitPrice: item.UnitPrice,
                        Quantity: item.Quantity,
                        LineTotal: lineTotal,
                        IsAvailable: variant.IsActive && variant.IsVisible,
                        IsLowStock: variant.StockStatus == Domain.Enums.StockStatus.LowStock
                    );

                    cartItems.Add(cartItem);
                }
            }

            return new CartDto(
                CartToken: cart.CartToken,
                Currency: cart.Currency,
                Items: cartItems,
                TotalItems: cart.Items.Sum(x => x.Quantity),
                TotalAmount: totalAmount
            );
        }
    }
}
