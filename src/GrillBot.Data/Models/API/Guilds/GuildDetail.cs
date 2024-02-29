using Discord;
using GrillBot.Data.Models.API.Channels;
using GrillBot.Data.Models.API.Users;
using System;
using System.Collections.Generic;
using GrillBot.Database.Models.Guilds;

namespace GrillBot.Data.Models.API.Guilds;

/// <summary>
/// Detailed information about guild.
/// </summary>
public class GuildDetail : Guild
{
    /// <summary>
    /// Datetime of guild creation in UTC.
    /// </summary>
    public DateTimeOffset? CreatedAt { get; set; }

    /// <summary>
    /// Icon URL
    /// </summary>
    public string? IconUrl { get; set; }

    /// <summary>
    /// Owner of guild
    /// </summary>
    public User Owner { get; set; } = null!;

    /// <summary>
    /// Premium level
    /// </summary>
    public PremiumTier? PremiumTier { get; set; }

    /// <summary>
    /// Vanity invite url if exists.
    /// </summary>
    public string? VanityUrl { get; set; }

    /// <summary>
    /// Muted role used to unverify.
    /// </summary>
    public Role? MutedRole { get; set; }

    /// <summary>
    /// Premium user role.
    /// </summary>
    public Role? BoosterRole { get; set; }

    /// <summary>
    /// Admin channel.
    /// </summary>
    public Channel? AdminChannel { get; set; }

    /// <summary>
    /// Channel for emote suggestions.
    /// </summary>
    public Channel? EmoteSuggestionChannel { get; set; }

    /// <summary>
    /// Channel for public votes.
    /// </summary>
    public Channel? VoteChannel { get; set; }

    public Channel? BotRoomChannel { get; set; }

    /// <summary>
    /// Maximum count of members.
    /// </summary>
    public int? MaxMembers { get; set; }

    /// <summary>
    /// Maximum online members.
    /// </summary>
    public int? MaxPresences { get; set; }

    /// <summary>
    /// Maximum members with webcam
    /// </summary>
    public int? MaxVideoChannelUsers { get; set; }

    /// <summary>
    /// Maximum bitrate
    /// </summary>
    public int MaxBitrate { get; set; }

    /// <summary>
    /// Maximum upload limit.
    /// </summary>
    public int MaxUploadLimit { get; set; }

    public DateTime? EmoteSuggestionsFrom { get; set; }
    public DateTime? EmoteSuggestionsTo { get; set; }

    public Dictionary<UserStatus, int> UserStatusReport { get; set; } = new();
    public Dictionary<ClientType, int> ClientTypeReport { get; set; } = new();
    public GuildDatabaseReport DatabaseReport { get; set; } = null!;
    public Role? AssociationRole { get; set; }
}
