#nullable enable
using TrendplusProdavnica.Application.Wishlist.Dtos;

namespace TrendplusProdavnica.Application.Wishlist.Services
{
    public interface IWishlistService
    {
        Task<WishlistDto> CreateWishlistAsync();
        Task<WishlistDto?> GetWishlistAsync(string wishlistToken);
        Task<WishlistDto> AddItemAsync(string wishlistToken, AddToWishlistRequest request);
        Task<WishlistDto> RemoveItemAsync(string wishlistToken, long productId);
        Task<WishlistDto> ClearAsync(string wishlistToken);
    }
}
