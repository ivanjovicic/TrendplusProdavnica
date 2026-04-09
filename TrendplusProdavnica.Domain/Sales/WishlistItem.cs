#nullable enable

namespace TrendplusProdavnica.Domain.Sales
{
    public class WishlistItem
    {
        public int Id { get; set; }
        public int WishlistId { get; set; }
        public long ProductId { get; set; }
        public DateTime AddedAtUtc { get; set; }

        // Navigation
        public Wishlist? Wishlist { get; set; }
    }
}
