using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Linq;

namespace GrillBot.Data.Helpers
{
    static public class DiscordHelper
    {
        static public GatewayIntents GetAllIntents()
        {
            return Enum.GetValues<GatewayIntents>().Aggregate((result, next) => result | next);
        }

        static public ChannelType? GetChannelType(IChannel channel)
        {
            if (channel == null) return null;

            return channel switch
            {
                SocketTextChannel => ChannelType.Text,
                SocketVoiceChannel => ChannelType.Voice,
                SocketCategoryChannel => ChannelType.Category,
                _ => null
            };
        }
    }
}
