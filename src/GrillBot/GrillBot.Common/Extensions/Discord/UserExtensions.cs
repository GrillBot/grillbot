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

    public static string GetDisplayName(this IUser user, bool withDiscriminator = true)
    {
        return user switch
        {
            null => "Neznámý uživatel",
            IGuildUser sgu when !string.IsNullOrEmpty(sgu.Nickname) => sgu.Nickname,
            _ => withDiscriminator ? $"{user.Username}#{user.Discriminator}" : user.Username
        };
    }

    private static bool HaveAnimatedAvatar(this IUser user) => user.AvatarId?.StartsWith("a_") ?? false;
    public static string CreateProfilePicFilename(this IUser user, int size) => $"{user.Id}_{user.AvatarId ?? user.Discriminator}_{size}.{(user.HaveAnimatedAvatar() ? "gif" : "png")}";

    public static IRole? GetHighestRole(this IGuildUser user, bool requireColor = false)
    {
        var roles = user.GetRoles();
        if (requireColor)
            roles = roles.Where(o => o.Color != Color.Default);

        return roles.MaxBy(o => o.Position);
    }

    public static Task TryAddRoleAsync(this IGuildUser user, IRole role)
    {
        return user.RoleIds.Any(o => o == role.Id) ? Task.CompletedTask : user.AddRoleAsync(role);
    }

    public static Task TryRemoveRoleAsync(this IGuildUser user, IRole role)
    {
        return user.RoleIds.All(o => o != role.Id) ? Task.CompletedTask : user.RemoveRoleAsync(role);
    }

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

    public static bool CanManageInvites(this IGuildUser user)
        => user.GuildPermissions.CreateInstantInvite && user.GuildPermissions.ManageGuild;
}
