using GrillBot.Data.Models.API.Guilds;

namespace GrillBot.Data.Models.API.Users;

public class UserPointsItem
{
    public User User { get; set; }
    public Guild Guild { get; set; }

    public string Nickname { get; set; }
    public long Points { get; set; }

    public UserPointsItem() { }

    public UserPointsItem(Database.Entity.GuildUser user)
    {
        User = new User(user.User);
        Guild = new Guild(user.Guild);
        Nickname = user.Nickname;
        Points = user.Points;
    }
}
