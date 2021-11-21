using System;
using System.Security.Claims;

namespace GrillBot.App.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static ulong GetUserId(this ClaimsPrincipal user)
        {
            if (user?.Identity == null) return default;
            var identifier = user.FindFirstValue(ClaimTypes.NameIdentifier);
            return string.IsNullOrEmpty(identifier) ? default : Convert.ToUInt64(identifier);
        }

        public static string GetUserRole(this ClaimsPrincipal user) => user.FindFirstValue(ClaimTypes.Role);

        public static bool HaveUserPermission(this ClaimsPrincipal user) => user.GetUserRole() == "User";
        public static bool HaveAdminPermission(this ClaimsPrincipal user) => user.GetUserRole() == "Admin";
    }
}
