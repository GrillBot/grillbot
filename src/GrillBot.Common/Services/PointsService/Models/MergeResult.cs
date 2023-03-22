namespace GrillBot.Common.Services.PointsService.Models;

public class MergeResult
{
    public int ExpiredCount { get; set; }
    public int MergedCount { get; set; }
    public string Duration { get; set; } = null!;
}
