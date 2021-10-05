using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GrillBot.Data.Extensions.Discord;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace GrillBot.App.Extensions.Discord
{
    static public class MessageExtensions
    {
        static public bool IsCommand(this IUserMessage message, IUser user, string prefix)
        {
            int argPos = 0;
            return IsCommand(message, ref argPos, user, prefix);
        }

        static public bool IsCommand(this IUserMessage message, ref int argumentPosition, IUser user, string prefix)
        {
            if (message.HasMentionPrefix(user, ref argumentPosition))
                return true;

            return message.Content.Length > prefix.Length && message.HasStringPrefix(prefix, ref argumentPosition);
        }

        static public bool TryLoadMessage(this SocketMessage message, out SocketUserMessage userMessage)
        {
            userMessage = null;

            if (message is not SocketUserMessage userMsg || !message.Author.IsUser())
                return false;

            userMessage = userMsg;
            return true;
        }

        static public IEnumerable<Emote> GetEmotesFromMessage(this IMessage message, List<GuildEmote> supportedEmotes = null)
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

        static public async Task<byte[]> DownloadAsync(this IAttachment attachment)
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
}
