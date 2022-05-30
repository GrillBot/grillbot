using Discord;

namespace GrillBot.Common.Extensions.Discord;

public static class UserExtensions
{
    public static string GetUserAvatarUrl(this IUser user, ushort size = 128)
        => user.GetAvatarUrl(size: size) ?? user.GetDefaultAvatarUrl();

    static public string GetFullName(this IUser user)
    {
        if (user is IGuildUser sgu && !string.IsNullOrEmpty(sgu.Nickname))
            return $"{sgu.Nickname} ({sgu.Username}#{sgu.Discriminator})";

        return $"{user.Username}#{user.Discriminator}";
    }
}
