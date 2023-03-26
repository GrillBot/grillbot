namespace GrillBot.Common.Services.PointsService.Models;

public class PointsChartItem
{
    public DateOnly Day { get; set; }
    public long MessagePoints { get; set; }
    public long ReactionPoints { get; set; }
}
