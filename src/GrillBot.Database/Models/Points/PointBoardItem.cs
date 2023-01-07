using GrillBot.Database.Entity;

namespace GrillBot.Database.Models.Points;

public class PointBoardItem
{
    public string GuildId { get; set; } = null!;
    public string UserId { get; set; } = null!;
    
    public GuildUser GuildUser { get; set; } = null!;
    
    public long PointsYearBack { get; set; }
    public long PointsMonthBack { get; set; }
    public long PointsToday { get; set; }
    public long TotalPoints { get; set; }
}
