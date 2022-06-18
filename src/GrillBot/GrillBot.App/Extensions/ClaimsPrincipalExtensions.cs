using GrillBot.Common.Extensions;
using System.Security.Claims;

namespace GrillBot.App.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        [Obsolete("Use ApiRequestContext")]
        public static ulong GetUserId(this ClaimsPrincipal user)
        {
            if (user?.Identity == null) return default;
            var identifier = user.FindFirstValue(ClaimTypes.NameIdentifier);
            return string.IsNullOrEmpty(identifier) ? default : identifier.ToUlong();
        }
        
        [Obsolete("Use ApiRequestContext")]
        public static string GetUserRole(this ClaimsPrincipal user) => user.FindFirstValue(ClaimTypes.Role);

        [Obsolete("Use ApiRequestContext")]
        public static bool HaveUserPermission(this ClaimsPrincipal user) => user.GetUserRole() == "User";
    }
}
