using Discord;
using Discord.Commands;

namespace GrillBot.Common.Extensions.Discord;

public static class MessageExtensions
{
    public static bool IsInteractionCommand(this IMessage message)
        => message.Type is MessageType.ApplicationCommand or MessageType.ContextMenuCommand;

    public static bool IsCommand(this IMessage message, IUser user, string prefix)
    {
        var argPos = 0;
        return IsCommand(message, ref argPos, user, prefix);
    }

    public static bool IsCommand(this IMessage message, ref int argumentPosition, IUser user, string prefix)
    {
        var isInteractionCommand = message.IsInteractionCommand();
        if (isInteractionCommand) return true;

        if (message is not IUserMessage msg || message.Content.Length < prefix.Length) return false;
        return msg.HasMentionPrefix(user, ref argumentPosition) || msg.HasStringPrefix(prefix, ref argumentPosition);
    }

    public static bool TryLoadMessage(this IMessage message, out IUserMessage? userMessage)
    {
        userMessage = null;

        if (message is not IUserMessage userMsg || !message.Author.IsUser())
            return false;

        userMessage = userMsg;
        return true;
    }

    public static IEnumerable<Emote> GetEmotesFromMessage(this IMessage message, List<GuildEmote>? supportedEmotes = null)
    {
        var query = message.Tags
            .Where(o => o.Type == TagType.Emoji) // Is emote
            .Select(o => o.Value) // Only emote property
            .OfType<Emote>() // Only emote type
            .Distinct(); // Without duplicates.

        if (supportedEmotes != null)
            query = query.Where(e => supportedEmotes.Any(x => x.IsEqual(e))); // Only supported emotes.

        return query;
    }

    public static async Task<byte[]?> DownloadAsync(this IAttachment attachment)
    {
        using var httpClient = new HttpClient();

        try
        {
            return await httpClient.GetByteArrayAsync(attachment.Url);
        }
        catch (HttpRequestException)
        {
            try
            {
                return await httpClient.GetByteArrayAsync(attachment.ProxyUrl);
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }
    }
}
