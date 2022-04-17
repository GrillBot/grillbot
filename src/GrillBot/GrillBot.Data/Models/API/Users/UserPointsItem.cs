using GrillBot.Data.Models.API.Guilds;

namespace GrillBot.Data.Models.API.Users;

public class UserPointsItem
{
    public User User { get; set; }
    public Guild Guild { get; set; }

    public string Nickname { get; set; }
    public long Points { get; set; }
}
