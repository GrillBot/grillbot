using GrillBot.Data.Models.API.Guilds;
using GrillBot.Data.Models.API.Users;
using System;
using System.Collections.Generic;

namespace GrillBot.Data.Models.API.Unverify;

/// <summary>
/// Unverify user profile about current unverify.
/// </summary>
public class UnverifyUserProfile : UnverifyInfo
{
    /// <summary>
    /// User that have unverify.
    /// </summary>
    public User User { get; set; }

    /// <summary>
    /// Removed roles.
    /// </summary>
    public List<Role> RolesToRemove { get; set; }

    /// <summary>
    /// Keeped roles.
    /// </summary>
    public List<Role> RolesToKeep { get; set; }

    /// <summary>
    /// Keeped channels.
    /// </summary>
    public List<string> ChannelsToKeep { get; set; }

    /// <summary>
    /// Removed channels.
    /// </summary>
    public List<string> ChannelsToRemove { get; set; }

    public Guild Guild { get; set; }
}
