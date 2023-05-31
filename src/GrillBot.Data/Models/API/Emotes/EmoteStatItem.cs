using System;

namespace GrillBot.Data.Models.API.Emotes;

public class EmoteStatItem
{
    public EmoteItem Emote { get; set; } = null!;
    public long UseCount { get; set; }
    public DateTime FirstOccurence { get; set; }
    public DateTime LastOccurence { get; set; }
    public int UsedUsersCount { get; set; }
}
