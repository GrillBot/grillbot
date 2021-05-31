using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace GrillBot.App.Extensions.Discord
{
    static public class MessageExtensions
    {
        static public bool IsCommand(this SocketUserMessage message, ref int argumentPosition, IUser botUser, string prefix)
        {
            if (message.HasMentionPrefix(botUser, ref argumentPosition))
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
    }
}
