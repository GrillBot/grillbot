using GrillBot.Data.Models.API.Guilds;
using GrillBot.Data.Models.API.Users;
using System;
using System.Collections.Generic;

namespace GrillBot.Data.Models.API.Unverify;

/// <summary>
/// Unverify user profile about current unverify.
/// </summary>
public class UnverifyUserProfile
{
    /// <summary>
    /// User that have unverify.
    /// </summary>
    public User User { get; set; }

    /// <summary>
    /// Start of unverify.
    /// </summary>
    public DateTime Start { get; set; }

    /// <summary>
    /// End of unverify.
    /// </summary>
    public DateTime End { get; set; }

    /// <summary>
    /// End to
    /// </summary>
    public TimeSpan EndTo { get; set; }

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

    /// <summary>
    /// Reason of remove.
    /// </summary>
    public string Reason { get; set; }

    /// <summary>
    /// Is this unverify selfunverify?
    /// </summary>
    public bool IsSelfUnverify { get; set; }

    public Guild Guild { get; set; }
}
