namespace TrendplusProdavnica.Application.Admin.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TrendplusProdavnica.Application.Admin.Models;

public interface IJwtTokenService
{
    string GenerateAccessToken(JwtClaimsPayload claims);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateToken(string token);
    bool ValidateRefreshToken(string token);
}

public class JwtTokenService : IJwtTokenService
{
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;
    private readonly int _refreshTokenExpirationDays;

    public JwtTokenService(IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("Jwt");
        _secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        _issuer = jwtSettings["Issuer"] ?? "TrendplusProdavnica";
        _audience = jwtSettings["Audience"] ?? "TrendplusProdavnica.Admin";
        _expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");
        _refreshTokenExpirationDays = int.Parse(jwtSettings["RefreshTokenExpirationDays"] ?? "7");
    }

    public string GenerateAccessToken(JwtClaimsPayload claims)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secretKey);

        var claimsList = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, claims.UserId.ToString()),
            new(ClaimTypes.Email, claims.Email),
            new(ClaimTypes.Name, claims.FullName),
            new(ClaimTypes.Role, claims.Role),
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claimsList),
            Expires = DateTime.UtcNow.AddMinutes(_expirationMinutes),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return principal;
        }
        catch
        {
            return null;
        }
    }

    public bool ValidateRefreshToken(string token)
    {
        // In production, refresh tokens should be validated against a database
        // For now, we'll just check if it's not empty
        return !string.IsNullOrWhiteSpace(token) && token.Length > 20;
    }
}
