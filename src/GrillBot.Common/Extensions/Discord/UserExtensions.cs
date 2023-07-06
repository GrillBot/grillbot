﻿using Discord;

namespace GrillBot.Common.Extensions.Discord;

public static class UserExtensions
{
    public static string GetUserAvatarUrl(this IUser user, ushort size = 128)
        => user.GetAvatarUrl(size: size) ?? user.GetDefaultAvatarUrl();

    public static string GetFullName(this IUser user)
    {
        if (user is IGuildUser guildUser && !string.IsNullOrEmpty(guildUser.Nickname))
        {
            return !string.IsNullOrEmpty(user.GlobalName) && user.GlobalName != user.Username
                ? $"{guildUser.Nickname} ({user.GlobalName} / {user.Username})"
                : $"{guildUser.Nickname} ({user.Username})";
        }

        return !string.IsNullOrEmpty(user.GlobalName) && user.GlobalName != user.Username ? $"{user.GlobalName} ({user.Username})" : user.Username;
    }

    public static IEnumerable<IRole> GetRoles(this IGuildUser user, bool withEveryone = false)
    {
        var ids = withEveryone ? user.RoleIds : user.RoleIds.Where(o => user.Guild.EveryoneRole.Id != o);
        return ids.Select(user.Guild.GetRole).Where(o => o != null);
    }

    public static bool IsUser(this IUser user) => user is { IsBot: false, IsWebhook: false };

    public static string GetDisplayName(this IUser user)
    {
        if (user is IGuildUser guildUser && !string.IsNullOrEmpty(guildUser.Nickname))
            return guildUser.Nickname;
        return string.IsNullOrEmpty(user.GlobalName) ? user.Username : user.GlobalName;
    }

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
