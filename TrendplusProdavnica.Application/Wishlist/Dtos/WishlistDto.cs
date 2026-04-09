#nullable enable

namespace TrendplusProdavnica.Application.Wishlist.Dtos
{
    public class WishlistDto
    {
        public string WishlistToken { get; set; } = null!;
        public List<WishlistItemDto> Items { get; set; } = new();
        public int ItemCount { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }
}
