using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace GrillBot.Common.Extensions.Discord;

public static class ChannelExtensions
{
    public static string GetMention(this IChannel channel) => $"<#{channel.Id}>";

    public static async Task<bool> HaveAccessAsync(this IGuildChannel channel, IGuildUser user)
    {
        if (channel is IThreadChannel { CategoryId: not null } thread)
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
            .Where(o => o != null)
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

        if (channel is IVoiceChannel voiceChannel && another is IVoiceChannel anotherVoiceChannel)
        {
            if (voiceChannel.Bitrate != anotherVoiceChannel.Bitrate) return false;
            if (voiceChannel.CategoryId != anotherVoiceChannel.CategoryId) return false;
            if (voiceChannel.UserLimit != anotherVoiceChannel.UserLimit) return false;
        }

        if (channel is IThreadChannel thread && another is IThreadChannel anotherThread)
        {
            if (thread.IsArchived != anotherThread.IsArchived) return false;
            if (thread.IsLocked != anotherThread.IsLocked) return false;
            if (thread.ArchiveTimestamp != anotherThread.ArchiveTimestamp) return false;
            if (thread.AutoArchiveDuration != anotherThread.AutoArchiveDuration) return false;
        }

        if (channel is IForumChannel forum && another is IForumChannel anotherForum)
        {
            if (forum.IsNsfw != anotherForum.IsNsfw) return false;
            if (forum.Topic != anotherForum.Topic) return false;
            if (!forum.Tags.Select(o => o.Id).SequenceEqual(anotherForum.Tags.Select(o => o.Id))) return false;
            if (forum.DefaultAutoArchiveDuration != anotherForum.DefaultAutoArchiveDuration) return false;
        }

        return true;
    }

    public static bool HaveCategory(this IGuildChannel channel)
        => channel is INestedChannel { CategoryId: { } };

    public static IChannel? GetCategory(this IGuildChannel channel)
    {
        return channel switch
        {
            SocketCategoryChannel socketCategoryChannel => socketCategoryChannel,
            RestCategoryChannel restCategoryChannel => restCategoryChannel,
            SocketThreadChannel socketThreadChannel => socketThreadChannel.ParentChannel,
            SocketVoiceChannel socketVoiceChannel => socketVoiceChannel.Category,
            SocketTextChannel socketTextChannel => socketTextChannel.Category,
            SocketForumChannel socketForumChannel => socketForumChannel.Category,
            _ => null
        };
    }
}
