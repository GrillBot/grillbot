using Discord;
using System;
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

        if (Uri.IsWellFormedUriString(reference, UriKind.Absolute))
        {
            var uriMatch = DiscordMessageUriRegex.Match(reference);

            if (uriMatch.Success)
            {
                return new MessageReference(
                    Convert.ToUInt64(uriMatch.Groups[3].Value),
                    channelId ?? Convert.ToUInt64(uriMatch.Groups[2].Value),
                    guildId ?? Convert.ToUInt64(uriMatch.Groups[1].Value)
                );
            }
        }

        return null;
    }
}
