using System;

namespace GrillBot.Database.Models.Emotes;

public class EmoteStatItem
{
    public string EmoteId { get; set; } = null!;
    public long UseCount { get; set; }
    public DateTime FirstOccurence { get; set; }
    public DateTime LastOccurence { get; set; }
    public int UsedUsersCount { get; set; }
}
