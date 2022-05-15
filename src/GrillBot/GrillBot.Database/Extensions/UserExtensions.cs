using Discord;
using Discord.WebSocket;

namespace GrillBot.Database.Extensions;

public static class UserExtensions
{
    public static UserStatus GetStatus(this IUser user)
        => FixStatus(user.Status);

    public static UserStatus GetStatus(this IPresence presence)
        => FixStatus(presence.Status);

    public static UserStatus FixStatus(this UserStatus status)
    {
        if (status == UserStatus.Invisible) return UserStatus.Offline;
        else if (status == UserStatus.AFK) return UserStatus.Idle;
        return status;
    }
}
