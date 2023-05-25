using System;
using GrillBot.Data.Models.API.Guilds;
using GrillBot.Data.Models.API.Users;

namespace GrillBot.Data.Models.API.Emotes;

public class EmoteStatsUserListItem
{
    public User User { get; set; } = null!;
    public Guild Guild { get; set; } = null!;

    public long UseCount { get; set; }
    public DateTime FirstOccurence { get; set; }
    public DateTime LastOccurence { get; set; }
}
