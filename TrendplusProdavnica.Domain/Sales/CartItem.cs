#nullable enable
using TrendplusProdavnica.Domain.Common;
using TrendplusProdavnica.Domain.Catalog;

namespace TrendplusProdavnica.Domain.Sales
{
    /// <summary>
    /// Single item in a shopping cart.
    /// References a ProductVariant and tracks quantity and unit price at time of addition.
    /// </summary>
    public class CartItem : EntityBase
    {
        /// <summary>
        /// Cart this item belongs to
        /// </summary>
        public long CartId { get; set; }

        /// <summary>
        /// Product variant being added to cart
        /// </summary>
        public long ProductVariantId { get; set; }

        /// <summary>
        /// Quantity of this variant in the cart
        /// </summary>
        public int Quantity { get; set; } = 1;

        /// <summary>
        /// Unit price of the variant at time of addition to cart
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Navigation: ProductVariant
        /// </summary>
        public ProductVariant? ProductVariant { get; set; }
    }
}
