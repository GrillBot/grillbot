using Discord;
using Discord.WebSocket;
using GrillBot.Data.Extensions.Discord;
using GrillBot.Data.Helpers;

namespace GrillBot.Data.Models.API.Channels
{
    public class GuildChannel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ChannelType? Type { get; set; }

        public GuildChannel() { }

        public GuildChannel(SocketGuildChannel channel)
        {
            Id = channel.Id.ToString();
            Name = channel.Name;
            Type = DiscordHelper.GetChannelType(channel);

            var category = channel.GetCategory();
            if (category != null)
                Name += $" ({category.Name})";
        }
    }
}
