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

    public bool IsAdmin() => LoggedUserData?.IsInRole("Admin") ?? false;
    public bool IsPublic() => LoggedUserData?.IsInRole("User") ?? false;
}
