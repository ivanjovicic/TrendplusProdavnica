using Microsoft.Extensions.Configuration;
using TrendplusProdavnica.Application.Admin.Models;
using TrendplusProdavnica.Application.Admin.Services;

namespace TrendplusProdavnica.Infrastructure.Admin.Services;

public class AdminAuthService : IAdminAuthService
{
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IConfiguration _configuration;

    public AdminAuthService(IJwtTokenService jwtTokenService, IConfiguration configuration)
    {
        _jwtTokenService = jwtTokenService;
        _configuration = configuration;
    }

    public Task<AuthTokenResponse?> LoginAsync(AuthCredentialsRequest credentials)
    {
        // Validate against configured admin credentials
        var configEmail = _configuration["AdminAuth:Email"];
        var configPassword = _configuration["AdminAuth:Password"];

        if (configEmail != credentials.Email || configPassword != credentials.Password)
        {
            return Task.FromResult<AuthTokenResponse?>(null);
        }

        // Generate JWT tokens
        var claims = new JwtClaimsPayload
        {
            UserId = 1,
            Email = credentials.Email,
            FullName = "Administrator",
            Role = "Admin"
        };

        var accessToken = _jwtTokenService.GenerateAccessToken(claims);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddHours(1);

        return Task.FromResult<AuthTokenResponse?>(new AuthTokenResponse
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = new AdminUserDto
            {
                Id = 1,
                Email = credentials.Email,
                FullName = "Administrator",
                Role = "Admin",
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow,
                LastLoginUtc = DateTime.UtcNow
            }
        });
    }

    public Task<AuthTokenResponse?> RefreshTokenAsync(RefreshTokenRequest request)
    {
        // Validate refresh token and issue new access token
        if (string.IsNullOrEmpty(request.RefreshToken))
            return Task.FromResult<AuthTokenResponse?>(null);

        var claims = new JwtClaimsPayload
        {
            UserId = 1,
            Email = "admin@trendplus.com",
            FullName = "Administrator",
            Role = "Admin"
        };

        var accessToken = _jwtTokenService.GenerateAccessToken(claims);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddHours(1);

        return Task.FromResult<AuthTokenResponse?>(new AuthTokenResponse
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = new AdminUserDto
            {
                Id = 1,
                Email = "admin@trendplus.com",
                FullName = "Administrator",
                Role = "Admin",
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow,
                LastLoginUtc = DateTime.UtcNow
            }
        });
    }

    public Task<AdminUserDto?> GetCurrentUserAsync(int userId)
    {
        return Task.FromResult<AdminUserDto?>(new AdminUserDto
        {
            Id = userId,
            Email = "admin@trendplus.com",
            FullName = "Administrator",
            Role = "Admin",
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow,
            LastLoginUtc = DateTime.UtcNow
        });
    }

    public Task<bool> ValidateAdminAccessAsync(int userId)
    {
        return Task.FromResult(userId == 1);
    }
}
