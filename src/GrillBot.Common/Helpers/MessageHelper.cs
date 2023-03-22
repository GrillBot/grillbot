using System.Text;
using Discord;
using GrillBot.Common.Extensions.Discord;
using NeoSmart.Unicode;
using Emoji = Discord.Emoji;

namespace GrillBot.Common.Helpers;

public static class MessageHelper
{
    public static IEnumerable<object> ParseMessage(string message)
    {
        var index = 0;

        while (index < message.Length)
        {
            if (message[index] == '<')
            {
                // Begin of "tag"
                var tagPart = new StringBuilder();
                for (; index < message.Length && message[index] != '>'; index++) tagPart.Append(message[index]);

                var tag = tagPart.Append(message[index]).ToString();
                index++;

                if (Emote.TryParse(tag, out var emote))
                {
                    yield return emote;
                }
                else
                {
                    foreach (var item in ProcessString(tag))
                        yield return item;
                }
            }
            else
            {
                var partBuilder = new StringBuilder();
                for (; index < message.Length && message[index] != '<'; index++) partBuilder.Append(message[index]);

                var part = partBuilder.ToString();
                foreach (var item in ProcessString(part))
                    yield return item;
            }
        }
    }

    private static IEnumerable<object> ProcessString(string str)
    {
        foreach (var codepoint in str.Codepoints())
        {
            if (!NeoSmart.Unicode.Emoji.IsEmoji(codepoint.AsString()))
            {
                yield return codepoint.AsString();
            }
            else
            {
                if (Emoji.TryParse(codepoint.AsString(), out var emoji))
                    yield return emoji;
                else
                    yield return codepoint.AsString();
            }
        }
    }

    public static bool CanSendAttachment(int attachmentSize, IGuild guild)
        => attachmentSize <= 2 * (guild.CalculateFileUploadLimit() * 1024 * 1024 / 3);
}
