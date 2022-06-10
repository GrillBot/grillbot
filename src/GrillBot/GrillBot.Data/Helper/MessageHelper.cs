using Discord;
using GrillBot.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace GrillBot.Data.Helper;

public static class MessageHelper
{
    public static Regex DiscordMessageUriRegex { get; } = new(@"https:\/\/discord\.com\/channels\/(@me|\d*)\/(\d+)\/(\d+)");

    public static MessageReference CreateMessageReference(string reference, ulong? channelId = null, ulong? guildId = null)
    {
        if (string.IsNullOrEmpty(reference))
            return null;

        if (ulong.TryParse(reference, out var messageId))
            return new MessageReference(messageId, channelId, guildId);

        if (!Uri.IsWellFormedUriString(reference, UriKind.Absolute))
            return null;

        var uriMatch = DiscordMessageUriRegex.Match(reference);
        if (uriMatch.Success)
        {
            return new MessageReference(
                uriMatch.Groups[3].Value.ToUlong(),
                channelId ?? uriMatch.Groups[2].Value.ToUlong(),
                guildId ?? uriMatch.Groups[1].Value.ToUlong()
            );
        }

        return null;
    }

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
