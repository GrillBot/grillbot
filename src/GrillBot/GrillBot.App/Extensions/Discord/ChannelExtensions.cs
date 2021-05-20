using Discord;

namespace GrillBot.App.Extensions.Discord
{
    static public class ChannelExtensions
    {
        static public string GetMention(this IChannel channel) => $"<#{channel.Id}>";
    }
}
