using Discord;

namespace GrillBot.Common.Extensions.Discord;

public static class UserExtensions
{
    public static string GetUserAvatarUrl(this IUser user, ushort size = 128)
        => user.GetAvatarUrl(size: size) ?? user.GetDefaultAvatarUrl();

    public static string GetFullName(this IUser user)
    {
        if (user is IGuildUser sgu && !string.IsNullOrEmpty(sgu.Nickname))
            return $"{sgu.Nickname} ({sgu.Username}#{sgu.Discriminator})";

        return $"{user.Username}#{user.Discriminator}";
    }

    public static IEnumerable<IRole> GetRoles(this IGuildUser user, bool withEveryone = false)
    {
        var ids = withEveryone ? user.RoleIds : user.RoleIds.Where(o => user.Guild.EveryoneRole.Id != o);
        return ids.Select(user.Guild.GetRole).Where(o => o != null);
    }

    public static bool IsUser(this IUser user) => !user.IsBot && !user.IsWebhook;
}
