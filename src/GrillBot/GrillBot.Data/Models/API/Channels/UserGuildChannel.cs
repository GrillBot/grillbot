using System;

namespace GrillBot.Data.Models.API.Channels
{
    public class UserGuildChannel
    {
        public Channel Channel { get; set; }
        public long Count { get; set; }
        public DateTime FirstMessageAt { get; set; }
        public DateTime LastMessageAt { get; set; }

        public UserGuildChannel() { }

        public UserGuildChannel(Database.Entity.GuildUserChannel channel)
        {
            Channel = new Channel(channel.Channel);
            Count = channel.Count;
            FirstMessageAt = channel.FirstMessageAt;
            LastMessageAt = channel.LastMessageAt;
        }
    }
}
