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

    public string Language { get; set; } = "cs";
    
    public ulong GetUserId()
    {
        if (LoggedUserData == null)
            return LoggedUser?.Id ?? 0;

        var userId = LoggedUserData.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return string.IsNullOrEmpty(userId) ? 0 : Convert.ToUInt64(userId);
    }

    public bool IsPublic()
        => LoggedUserData?.IsInRole("User") ?? false;

    public string? GetUserRole()
        => LoggedUserData?.FindFirst(ClaimTypes.Role)?.Value;

    public string? GetUsername()
        => LoggedUser is null ? LoggedUserData?.FindFirst(ClaimTypes.Name)?.Value : LoggedUser.Username;
}
