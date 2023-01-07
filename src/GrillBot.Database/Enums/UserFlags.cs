using System;

namespace GrillBot.Database.Enums;

/// <summary>
/// User specific flags.
/// </summary>
[Flags]
public enum UserFlags
{
    None = 0,

    /// <summary>
    /// User have full permissions.
    /// </summary>
    BotAdmin = 1,

    /// <summary>
    /// User have access to webadmin.
    /// </summary>
    WebAdmin = 2,

    /// <summary>
    /// User is not standard user.
    /// </summary>
    NotUser = 4,

    /// <summary>
    /// User is logged in webadmin.
    /// </summary>
    WebAdminOnline = 8,

    /// <summary>
    /// Public web administration blocked.
    /// </summary>
    PublicAdministrationBlocked = 16,

    /// <summary>
    /// User is logged to public administration.
    /// </summary>
    PublicAdminOnline = 32,

    /// <summary>
    /// All commands for user is disabled.
    /// </summary>
    CommandsDisabled = 64,
    
    /// <summary>
    /// Points counting is disabled for the user.
    /// </summary>
    PointsDisabled = 128
}
