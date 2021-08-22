using System;

namespace GrillBot.Data.Models.API.Channels
{
    public class UserGuildChannel
    {
        public GuildChannel Channel { get; set; }
        public long Count { get; set; }
        public DateTime FirstMessageAt { get; set; }
        public DateTime LastMessageAt { get; set; }

        public UserGuildChannel() { }

        public UserGuildChannel(Database.Entity.GuildUserChannel channel)
        {
            Channel = new GuildChannel(channel.Channel);
            Count = channel.Count;
            FirstMessageAt = channel.FirstMessageAt;
            LastMessageAt = channel.LastMessageAt;
        }
    }
}
