using System;
using GrillBot.Data.Models.API.Guilds;
using GrillBot.Data.Models.API.Users;

namespace GrillBot.Data.Models.API.Points;

public class PointsTransaction
{
    public Guild Guild { get; set; } = null!;
    public User User { get; set; } = null!;

    public string MessageId { get; set; } = null!;
    public bool IsReaction { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Points { get; set; }

    public PointsMergeInfo? MergeInfo { get; set; }
}
