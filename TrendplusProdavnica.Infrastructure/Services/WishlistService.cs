#nullable enable
using TrendplusProdavnica.Application.Wishlist.Dtos;
using TrendplusProdavnica.Application.Wishlist.Services;
using TrendplusProdavnica.Domain.Sales;
using TrendplusProdavnica.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace TrendplusProdavnica.Infrastructure.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly TrendplusDbContext _dbContext;

        public WishlistService(TrendplusDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<WishlistDto> CreateWishlistAsync()
        {
            var wishlist = new Wishlist
            {
                WishlistToken = Guid.NewGuid().ToString("N"),
                CreatedAtUtc = DateTime.UtcNow,
                UpdatedAtUtc = DateTime.UtcNow
            };

            _dbContext.Wishlists.Add(wishlist);
            await _dbContext.SaveChangesAsync();

            return MapToDto(wishlist, new List<WishlistItemDto>());
        }

        public async Task<WishlistDto?> GetWishlistAsync(string wishlistToken)
        {
            var wishlist = await _dbContext.Wishlists
                .AsNoTracking()
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.WishlistToken == wishlistToken);

            if (wishlist == null)
                return null;

            var items = await GetWishlistItemsWithDetailsAsync(wishlist.Id);
            return MapToDto(wishlist, items);
        }

        public async Task<WishlistDto> AddItemAsync(string wishlistToken, AddToWishlistRequest request)
        {
            var wishlist = await _dbContext.Wishlists
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.WishlistToken == wishlistToken)
                ?? throw new KeyNotFoundException($"Wishlist {wishlistToken} not found");

            // Validate product exists
            var product = await _dbContext.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.ProductId)
                ?? throw new KeyNotFoundException($"Product {request.ProductId} not found");

            // Check if item already exists in wishlist
            var existingItem = wishlist.Items.FirstOrDefault(x => x.ProductId == request.ProductId);

            if (existingItem == null)
            {
                // Add new item
                var item = new WishlistItem
                {
                    WishlistId = wishlist.Id,
                    ProductId = request.ProductId,
                    AddedAtUtc = DateTime.UtcNow
                };
                wishlist.Items.Add(item);
                wishlist.UpdatedAtUtc = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
            }

            var items = await GetWishlistItemsWithDetailsAsync(wishlist.Id);
            return MapToDto(wishlist, items);
        }

        public async Task<WishlistDto> RemoveItemAsync(string wishlistToken, long productId)
        {
            var wishlist = await _dbContext.Wishlists
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.WishlistToken == wishlistToken)
                ?? throw new KeyNotFoundException($"Wishlist {wishlistToken} not found");

            var item = wishlist.Items.FirstOrDefault(x => x.ProductId == productId);

            if (item != null)
            {
                wishlist.Items.Remove(item);
                wishlist.UpdatedAtUtc = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
            }

            var items = await GetWishlistItemsWithDetailsAsync(wishlist.Id);
            return MapToDto(wishlist, items);
        }

        public async Task<WishlistDto> ClearAsync(string wishlistToken)
        {
            var wishlist = await _dbContext.Wishlists
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.WishlistToken == wishlistToken)
                ?? throw new KeyNotFoundException($"Wishlist {wishlistToken} not found");

            wishlist.Items.Clear();
            wishlist.UpdatedAtUtc = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            return MapToDto(wishlist, new List<WishlistItemDto>());
        }

        private async Task<List<WishlistItemDto>> GetWishlistItemsWithDetailsAsync(int wishlistId)
        {
            var items = await _dbContext.WishlistItems
                .AsNoTracking()
                .Where(x => x.WishlistId == wishlistId)
                .Join(
                    _dbContext.Products.AsNoTracking()
                        .Include(p => p.Variants)
                        .Include(p => p.Media),
                    wi => wi.ProductId,
                    p => p.Id,
                    (wi, p) => new { WishlistItem = wi, Product = p }
                )
                .Join(
                    _dbContext.Brands.AsNoTracking(),
                    x => x.Product.BrandId,
                    b => b.Id,
                    (x, b) => new { x.WishlistItem, x.Product, Brand = b }
                )
                .Select(x => new WishlistItemDto
                {
                    ProductId = x.Product.Id,
                    ProductSlug = x.Product.Slug,
                    ProductName = x.Product.Name,
                    BrandName = x.Brand.Name,
                    PrimaryImageUrl = x.Product.Media
                        .OrderBy(m => m.SortOrder)
                        .Where(m => m.IsPrimary)
                        .Select(m => m.Url)
                        .FirstOrDefault(),    
                    Price = x.Product.Variants
                        .Where(v => v.IsActive && v.IsVisible)
                        .Select(v => v.Price)
                        .Min(),
                    OldPrice = x.Product.Variants
                        .Where(v => v.IsActive && v.IsVisible && v.OldPrice.HasValue)
                        .Select(v => v.OldPrice)
                        .Min(),
                    IsInStock = x.Product.Variants.Any(v => v.IsActive && v.IsVisible && v.TotalStock > 0),
                    AddedAtUtc = x.WishlistItem.AddedAtUtc
                })
                .OrderByDescending(x => x.AddedAtUtc)
                .ToListAsync();

            return items;
        }

        private WishlistDto MapToDto(Wishlist wishlist, List<WishlistItemDto> items)
        {
            return new WishlistDto
            {
                WishlistToken = wishlist.WishlistToken,
                Items = items,
                ItemCount = items.Count,
                CreatedAtUtc = wishlist.CreatedAtUtc,
                UpdatedAtUtc = wishlist.UpdatedAtUtc
            };
        }
    }
}

