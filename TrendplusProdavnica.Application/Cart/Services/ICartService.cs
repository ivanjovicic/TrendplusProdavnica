#nullable enable
using TrendplusProdavnica.Application.Cart.Dtos;

namespace TrendplusProdavnica.Application.Cart.Services
{
    /// <summary>
    /// Service for managing shopping carts
    /// </summary>
    public interface ICartService
    {
        /// <summary>
        /// Create a new empty cart
        /// </summary>
        Task<CartDto> CreateCartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get existing cart by token
        /// </summary>
        /// <returns>Cart DTO or null if not found</returns>
        Task<CartDto?> GetCartAsync(string cartToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get existing cart by session id (optionally user scoped).
        /// </summary>
        Task<CartDto?> GetCartBySessionAsync(string sessionId, string? userId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Add item to cart (or increase quantity if variant already in cart)
        /// </summary>
        Task<CartDto> AddItemAsync(string cartToken, AddToCartRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Add item to cart identified by session id.
        /// </summary>
        Task<CartDto> AddItemBySessionAsync(AddToCartBySessionRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update quantity of an existing cart item
        /// </summary>
        Task<CartDto> UpdateItemQuantityAsync(string cartToken, long itemId, UpdateCartItemRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Update quantity by product variant in cart identified by session id.
        /// </summary>
        Task<CartDto> UpdateItemBySessionAsync(UpdateCartBySessionRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Remove item from cart
        /// </summary>
        Task<CartDto> RemoveItemAsync(string cartToken, long itemId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Remove item by product variant in cart identified by session id.
        /// </summary>
        Task<CartDto> RemoveItemBySessionAsync(RemoveFromCartRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Clear all items from cart
        /// </summary>
        Task<CartDto> ClearCartAsync(string cartToken, CancellationToken cancellationToken = default);
    }
}
