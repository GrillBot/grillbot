using System;

namespace GrillBot.Data.Models;

public class EmoteStatItem
{
    public string Id { get; set; }
    public int UsersCount { get; set; }
    public long UseCount { get; set; }
    public DateTime FirstOccurence { get; set; }
    public DateTime LastOccurence { get; set; }
}
