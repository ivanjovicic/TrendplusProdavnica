#nullable enable

namespace TrendplusProdavnica.Domain.Sales
{
    public class Wishlist
    {
        public int Id { get; set; }
        public string WishlistToken { get; set; } = null!;
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }

        // Navigation
        public ICollection<WishlistItem> Items { get; set; } = new List<WishlistItem>();
    }
}
