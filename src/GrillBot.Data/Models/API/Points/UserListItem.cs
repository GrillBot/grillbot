using GrillBot.Data.Models.API.Guilds;
using GrillBot.Data.Models.API.Users;

namespace GrillBot.Data.Models.API.Points;

public class UserListItem
{
    public Guild Guild { get; set; } = null!;
    public User User { get; set; } = null!;

    public long ActivePoints { get; set; }
    public long ExpiredPoints { get; set; }
    public long MergedPoints { get; set; }
    public bool PointsDeactivated { get; set; }
}
