using System;
using GrillBot.Data.Models.API.Guilds;
using GrillBot.Data.Models.API.Users;

namespace GrillBot.Data.Models.API.Points;

public class PointsTransaction
{
    public Guild Guild { get; set; }
    public User User { get; set; }
    
    public string MessageId { get; set; }
    public bool IsReaction { get; set; }
    public DateTime AssignedAt { get; set; }
    public int Points { get; set; }
}
