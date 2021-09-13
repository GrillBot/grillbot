using System;
using System.Security.Claims;

namespace GrillBot.App.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static ulong GetUserId(this ClaimsPrincipal user)
        {
            var identifier = user.FindFirstValue(ClaimTypes.NameIdentifier);
            return string.IsNullOrEmpty(identifier) ? default : Convert.ToUInt64(identifier);
        }
    }
}
