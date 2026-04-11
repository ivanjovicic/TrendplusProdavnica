#nullable enable
using System.Security.Claims;

namespace TrendplusProdavnica.Api.Infrastructure.Auth
{
    public static class ApiAuthorizationPolicies
    {
        public const string Admin = "AdminPolicy";
        public const string Operational = "OperationalPolicy";

        public static bool HasAdminAccess(ClaimsPrincipal? user)
        {
            if (user?.Identity?.IsAuthenticated != true)
            {
                return false;
            }

            return user.Claims.Any(claim =>
                (string.Equals(claim.Type, ClaimTypes.Role, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(claim.Type, "role", StringComparison.OrdinalIgnoreCase)) &&
                string.Equals(claim.Value, "Admin", StringComparison.OrdinalIgnoreCase));
        }
    }
}
