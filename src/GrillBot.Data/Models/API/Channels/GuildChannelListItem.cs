using GrillBot.Data.Models.API.Guilds;
using System;

namespace GrillBot.Data.Models.API.Channels;

public class GuildChannelListItem : Channel
{
    public Guild Guild { get; set; } = null!;

    public DateTime? FirstMessageAt { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public long MessagesCount { get; set; }
    public int CachedMessagesCount { get; set; }

    public int? RolePermissionsCount { get; set; }
    public int? UserPermissionsCount { get; set; }
    public int PinCount { get; set; }
}
