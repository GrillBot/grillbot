using GrillBot.Data.Models.API.Guilds;

namespace GrillBot.Data.Models.API.Users;

public class UserPointsItem
{
    public User User { get; set; }
    public Guild Guild { get; set; }

    public string Nickname { get; set; }
    
    public long PointsYearBack { get; set; }
    public long PointsMonthBack { get; set; }
    public long PointsToday { get; set; }
    public long TotalPoints { get; set; }
}
