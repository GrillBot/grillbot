using System.Security.Claims;
using Discord;

namespace GrillBot.Common.Models;

public class ApiRequestContext
{
    /// <summary>
    /// Metadata of currently logged user.
    /// </summary>
    public ClaimsPrincipal? LoggedUserData { get; set; }

    /// <summary>
    /// Logged user discord entity.
    /// </summary>
    public IUser? LoggedUser { get; set; }

    public ulong GetUserId()
    {
        var userId = LoggedUserData?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrEmpty(userId) ? 0 : Convert.ToUInt64(userId);
    }

    public bool IsPublic()
        => LoggedUserData?.IsInRole("User") ?? false;

    public string? GetUserRole()
        => LoggedUserData?.FindFirst(ClaimTypes.Role)?.Value;
}
