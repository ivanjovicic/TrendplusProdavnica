namespace TrendplusProdavnica.Application.Admin.Models;

/// <summary>
/// Admin login credentials
/// </summary>
public class AuthCredentialsRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// JWT token response
/// </summary>
public class AuthTokenResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public AdminUserDto User { get; set; } = new();
}

/// <summary>
/// Admin user information
/// </summary>
public class AdminUserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "admin"; // "admin", "editor", "moderator"
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? LastLoginUtc { get; set; }
}

/// <summary>
/// JWT claims payload
/// </summary>
public class JwtClaimsPayload
{
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

/// <summary>
/// Refresh token request
/// </summary>
public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}
