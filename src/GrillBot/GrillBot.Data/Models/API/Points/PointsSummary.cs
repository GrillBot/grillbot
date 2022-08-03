using System;
using GrillBot.Data.Models.API.Guilds;
using GrillBot.Data.Models.API.Users;

namespace GrillBot.Data.Models.API.Points;

public class PointsSummary : PointsSummaryBase
{
    public Guild Guild { get; set; }
    public User User { get; set; }
}
