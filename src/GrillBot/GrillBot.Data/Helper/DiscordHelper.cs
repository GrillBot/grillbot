using Discord;
using Discord.WebSocket;
using System;

namespace GrillBot.Data.Helpers
{
    static public class DiscordHelper
    {
        [Obsolete("Use channel.GetChannelType() from Discord.NET extensions")]
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
