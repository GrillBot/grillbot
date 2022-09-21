using System.Text.RegularExpressions;
using Discord;

namespace GrillBot.Common.Helpers;

public static class MessageHelper
{
    public static Regex DiscordMessageUriRegex { get; } = new(@"https:\/\/discord\.com\/channels\/(@me|\d*)\/(\d+)\/(\d+)");

    public static string ClearEmotes(string content, IEnumerable<IEmote> emotes)
    {
        string Process(IEnumerable<IEmote> emotesData)
        {
            return emotesData
                .Distinct()
                .Select(o => o.ToString() ?? "")
                .Aggregate(content, (current, emoteId) => current.Replace(emoteId, ""))
                .Trim();
        }

        content = Process(emotes.ToList());
        content = Process(Emojis.PaginationEmojis);
        content = Process(Emojis.NumberToEmojiMap.Values);
        content = Process(Emojis.CharToEmojiMap.Values);
        content = Process(Emojis.CharToSignEmojiMap.Values);

        return content;
    }
}
