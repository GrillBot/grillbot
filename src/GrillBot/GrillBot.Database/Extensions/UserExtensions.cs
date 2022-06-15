using System.Text;
using Discord;
using GrillBot.Database.Entity;

namespace GrillBot.Database.Extensions;

public static class UserExtensions
{
    public static UserStatus GetStatus(this IUser user)
        => FixStatus(user.Status);

    public static UserStatus GetStatus(this IPresence presence)
        => FixStatus(presence.Status);

    private static UserStatus FixStatus(this UserStatus status)
    {
        return status switch
        {
            UserStatus.Invisible => UserStatus.Offline,
            UserStatus.AFK => UserStatus.Idle,
            _ => status
        };
    }

    public static string FullName(this GuildUser user, bool noDiscriminator = false)
    {
        var username = $"{user.User!.Username}{(noDiscriminator ? "" : $"#{user.User!.Discriminator}")}";
        return !string.IsNullOrEmpty(user.Nickname) ? $"{user.Nickname} ({username})" : username;
    }
}
