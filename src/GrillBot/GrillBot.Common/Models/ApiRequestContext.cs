﻿using System.Security.Claims;
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

    public ulong LoggedUserId => LoggedUser?.Id ?? 0;

    public bool IsPublic() => LoggedUserData?.IsInRole("User") ?? false;
}
