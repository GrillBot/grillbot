namespace GrillBot.Common.Services.PointsService.Models;

public class Leaderboard
{
    public List<BoardItem> Items { get; set; } = new();
    public int TotalItemsCount { get; set; }
}
