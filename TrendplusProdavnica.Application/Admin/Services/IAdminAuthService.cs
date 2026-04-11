#nullable enable
using System.Threading.Tasks;
using TrendplusProdavnica.Application.Admin.Models;

namespace TrendplusProdavnica.Application.Admin.Services
{
    public interface IAdminAuthService
    {
        Task<AuthTokenResponse?> LoginAsync(AuthCredentialsRequest credentials);
        Task<AuthTokenResponse?> RefreshTokenAsync(RefreshTokenRequest request);
        Task<AdminUserDto?> GetCurrentUserAsync(int userId);
        Task<bool> ValidateAdminAccessAsync(int userId);
    }
}
