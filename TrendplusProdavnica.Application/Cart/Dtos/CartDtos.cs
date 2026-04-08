#nullable enable
namespace TrendplusProdavnica.Application.Cart.Dtos
{
    /// <summary>
    /// Request model for adding an item to cart
    /// </summary>
    public record AddToCartRequest(
        long ProductVariantId,
        int Quantity
    );

    /// <summary>
    /// Request model for updating cart item quantity
    /// </summary>
    public record UpdateCartItemRequest(
        int Quantity
    );

    /// <summary>
    /// Individual cart item with product details
    /// </summary>
    public record CartItemDto(
        long ItemId,
        long ProductVariantId,
        string ProductSlug,
        string ProductName,
        string BrandName,
        decimal SizeEu,
        string? PrimaryImageUrl,
        decimal UnitPrice,
        int Quantity,
        decimal LineTotal,
        bool IsAvailable,
        bool IsLowStock
    );

    /// <summary>
    /// Complete shopping cart with items and totals
    /// </summary>
    public record CartDto(
        string CartToken,
        string Currency,
        IReadOnlyList<CartItemDto> Items,
        int TotalItems,
        decimal TotalAmount
    );
}
