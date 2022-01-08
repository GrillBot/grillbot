using Discord;
using Discord.WebSocket;

namespace GrillBot.Data.Helpers
{
    static public class DiscordHelper
    {
        static public ChannelType? GetChannelType(IChannel channel)
        {
            if (channel == null) return null;

            return channel switch
            {
                SocketThreadChannel thread => thread.IsPrivateThread ? ChannelType.PrivateThread : ChannelType.PublicThread,
                SocketStageChannel => ChannelType.Stage,
                SocketTextChannel => ChannelType.Text,
                SocketVoiceChannel => ChannelType.Voice,
                SocketCategoryChannel => ChannelType.Category,
                _ => null
            };
        }
    }
}
