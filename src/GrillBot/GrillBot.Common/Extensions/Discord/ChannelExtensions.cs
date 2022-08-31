using Discord;
using Discord.WebSocket;

namespace GrillBot.Common.Extensions.Discord;

public static class ChannelExtensions
{
    public static string GetMention(this IChannel channel) => $"<#{channel.Id}>";

    public static async Task<bool> HaveAccessAsync(this IGuildChannel channel, IGuildUser user)
    {
        if (channel is IThreadChannel { CategoryId: { } } thread)
            return await HaveAccessAsync(await channel.Guild.GetChannelAsync(thread.CategoryId.Value), user);

        if (channel.PermissionOverwrites == null || channel.PermissionOverwrites.Count == 0)
            return true;
        if (await channel.GetUserAsync(user.Id, CacheMode.CacheOnly) != null)
            return true;

        var overwrite = channel.GetPermissionOverwrite(user);
        if (overwrite != null)
        {
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (overwrite.Value.ViewChannel == PermValue.Allow)
                return true;
            if (overwrite.Value.ViewChannel == PermValue.Deny)
                return false;
        }

        var everyonePerm = channel.GetPermissionOverwrite(user.Guild.EveryoneRole);
        var isEveryonePerm = everyonePerm is { ViewChannel: PermValue.Allow or PermValue.Inherit };

        var userRoles = user.RoleIds
            .Where(o => o != user.Guild.EveryoneRole.Id)
            .Select(o => user.Guild.GetRole(o))
            .OrderByDescending(o => o.Position);

        foreach (var role in userRoles)
        {
            var roleOverwrite = channel.GetPermissionOverwrite(role);
            if (roleOverwrite == null) continue;

            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (roleOverwrite.Value.ViewChannel == PermValue.Deny && isEveryonePerm)
                return false;

            if (roleOverwrite.Value.ViewChannel == PermValue.Allow)
                return true;
        }

        return isEveryonePerm;
    }

    public static bool IsEqual(this IGuildChannel channel, IGuildChannel another)
    {
        if (channel.GetType() != another.GetType()) return false;
        if (channel.Id != another.Id) return false;
        if (channel.Name != another.Name) return false;
        if (channel.Position != another.Position) return false;

        if (channel is ITextChannel textChannel && another is ITextChannel anotherTextChannel)
        {
            if (textChannel.CategoryId != anotherTextChannel.CategoryId) return false;
            if (textChannel.IsNsfw != anotherTextChannel.IsNsfw) return false;
            if (textChannel.SlowModeInterval != anotherTextChannel.SlowModeInterval) return false;
            if (textChannel.Topic != anotherTextChannel.Topic) return false;
        }

        if (channel is not IVoiceChannel voiceChannel || another is not IVoiceChannel anotherVoiceChannel)
            return true;

        if (voiceChannel.Bitrate != anotherVoiceChannel.Bitrate) return false;
        if (voiceChannel.CategoryId != anotherVoiceChannel.CategoryId) return false;
        return voiceChannel.UserLimit == anotherVoiceChannel.UserLimit;
    }

    public static bool HaveCategory(this IGuildChannel channel)
        => channel is INestedChannel { CategoryId: { } };

    public static IChannel? GetCategory(this IGuildChannel channel)
    {
        return channel switch
        {
            SocketCategoryChannel categoryChannel => categoryChannel,
            SocketThreadChannel thread => thread.ParentChannel,
            SocketVoiceChannel voiceChannel => voiceChannel.Category,
            SocketTextChannel textChannel => textChannel.Category,
            _ => null
        };
    }
}
