using GrillBot.Data.Models.API.Guilds;
using GrillBot.Data.Models.API.Users;
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
    public User User { get; set; } = null!;

    /// <summary>
    /// Removed roles.
    /// </summary>
    public List<Role> RolesToRemove { get; set; } = new();

    /// <summary>
    /// Keeped roles.
    /// </summary>
    public List<Role> RolesToKeep { get; set; } = new();

    /// <summary>
    /// Keeped channels.
    /// </summary>
    public List<string> ChannelsToKeep { get; set; } = new();

    /// <summary>
    /// Removed channels.
    /// </summary>
    public List<string> ChannelsToRemove { get; set; } = new();

    public Guild Guild { get; set; } = null!;
}
