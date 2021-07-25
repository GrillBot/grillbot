using Discord.WebSocket;

namespace GrillBot.Data.Models.API.Channels
{
    public class GuildChannel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        public GuildChannel() { }

        public GuildChannel(SocketGuildChannel channel)
        {
            Id = channel.Id.ToString();
            Name = channel.Name;

            if (channel is SocketNewsChannel) Type = "News";
            else if (channel is SocketTextChannel) Type = "Text";
            else if (channel is SocketVoiceChannel) Type = "Voice";
            else Type = "Unknown";
        }
    }
}
