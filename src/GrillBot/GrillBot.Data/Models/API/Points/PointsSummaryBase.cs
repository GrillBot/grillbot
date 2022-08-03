using System;

namespace GrillBot.Data.Models.API.Points;

public class PointsSummaryBase
{
    public DateTime Day { get; set; }
    public long MessagePoints { get; set; }
    public long ReactionPoints { get; set; }
    public long TotalPoints { get; set; }
}
