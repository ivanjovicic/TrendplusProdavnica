#nullable enable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TrendplusProdavnica.Api.Infrastructure.Auth;
using TrendplusProdavnica.Application.Admin.Models;
using TrendplusProdavnica.Application.Admin.Services;

namespace TrendplusProdavnica.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/auth")]
    public class AuthAdminController : ControllerBase
    {
        private readonly IAdminAuthService _authService;

        public AuthAdminController(IAdminAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Admin login endpoint
        /// </summary>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<AuthTokenResponse>> Login([FromBody] AuthCredentialsRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { error = "Email and password are required" });
            }

            var result = await _authService.LoginAsync(request);
            if (result == null)
            {
                return Unauthorized(new { error = "Invalid credentials" });
            }

            return Ok(result);
        }

        /// <summary>
        /// Refresh JWT token
        /// </summary>
        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<ActionResult<AuthTokenResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(new { error = "Refresh token is required" });
            }

            var result = await _authService.RefreshTokenAsync(request);
            if (result == null)
            {
                return Unauthorized(new { error = "Invalid or expired refresh token" });
            }

            return Ok(result);
        }

        /// <summary>
        /// Get current admin user information
        /// </summary>
        [Authorize(Policy = ApiAuthorizationPolicies.Admin)]
        [HttpGet("me")]
        public async Task<ActionResult<AdminUserDto>> GetCurrentUser()
        {
            // Extract user ID from JWT token claims
            var userIdClaim = User.FindFirst("sub") ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
            
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                return Unauthorized(new { error = "User not authenticated" });
            }

            var user = await _authService.GetCurrentUserAsync(userId);
            if (user == null)
            {
                return Unauthorized(new { error = "User not found" });
            }

            return Ok(user);
        }

        /// <summary>
        /// Validate admin access for a user
        /// </summary>
        [Authorize(Policy = ApiAuthorizationPolicies.Admin)]
        [HttpGet("validate/{userId:int}")]
        public async Task<ActionResult<object>> ValidateAccess(int userId)
        {
            var isValid = await _authService.ValidateAdminAccessAsync(userId);
            return Ok(new { isValid });
        }
    }
}
