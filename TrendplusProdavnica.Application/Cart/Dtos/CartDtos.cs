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
    /// Request model for adding item to cart identified by session.
    /// </summary>
    public record AddToCartBySessionRequest(
        string SessionId,
        long ProductVariantId,
        int Quantity,
        string? UserId = null
    );

    /// <summary>
    /// Request model for removing item from cart identified by session.
    /// </summary>
    public record RemoveFromCartRequest(
        string SessionId,
        long ProductVariantId,
        string? UserId = null
    );

    /// <summary>
    /// Request model for updating quantity by product variant in a session cart.
    /// </summary>
    public record UpdateCartBySessionRequest(
        string SessionId,
        long ProductVariantId,
        int Quantity,
        string? UserId = null
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
    )
    {
        public decimal Price => UnitPrice;
    }

    /// <summary>
    /// Complete shopping cart with items and totals
    /// </summary>
    public record CartDto(
        long CartId,
        string? UserId,
        string? SessionId,
        string CartToken,
        string Currency,
        IReadOnlyList<CartItemDto> Items,
        int TotalItems,
        decimal TotalAmount
    )
    {
        public decimal TotalPrice => TotalAmount;
    }
}
