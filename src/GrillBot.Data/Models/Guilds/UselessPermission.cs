using Discord;
using GrillBot.Data.Enums;

namespace GrillBot.Data.Models.Guilds;

public class UselessPermission
{
    public IGuildChannel Channel { get; set; }
    public IGuildUser User { get; set; }
    public UselessPermissionType Type { get; }

    public UselessPermission(IGuildChannel channel, IGuildUser user, UselessPermissionType type)
    {
        Channel = channel;
        User = user;
        Type = type;
    }
}
