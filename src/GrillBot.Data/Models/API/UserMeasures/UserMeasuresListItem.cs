using GrillBot.Data.Models.API.Guilds;
using GrillBot.Data.Models.API.Users;

namespace GrillBot.Data.Models.API.UserMeasures;

public class UserMeasuresListItem : UserMeasuresItem
{
    public Guild Guild { get; set; } = null!;
    public User User { get; set; } = null!;
}
