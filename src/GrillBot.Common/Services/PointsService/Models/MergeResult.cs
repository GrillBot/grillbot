namespace GrillBot.Common.Services.PointsService.Models;

public class MergeResult
{
    public int ExpiredCount { get; set; }
    public int MergedCount { get; set; }
    public string Duration { get; set; } = null!;
    public int GuildCount { get; set; }
    public int UserCount { get; set; }
    public int TotalPoints { get; set; }
}
