#nullable enable

namespace TrendplusProdavnica.Application.Wishlist.Dtos
{
    public class WishlistItemDto
    {
        public long ProductId { get; set; }
        public string ProductSlug { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public string BrandName { get; set; } = null!;
        public string? PrimaryImageUrl { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public bool IsInStock { get; set; }
        public DateTime AddedAtUtc { get; set; }
    }
}
