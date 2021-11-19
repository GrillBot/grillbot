using GrillBot.Data.Models.API.Guilds;
using System;

namespace GrillBot.Data.Models.API.Channels
{
    public class ChannelboardItem
    {
        public GuildChannel Channel { get; set; }
        public long Count { get; set; }
        public DateTime LastMessageAt { get; set; }
        public DateTime FirstMessageAt { get; set; }

        public ChannelboardItem() { }

        public ChannelboardItem(Database.Entity.GuildChannel channel, long count,
            DateTime lastMessageAt, DateTime firstMessageAt)
        {
            Channel = new GuildChannel(channel);
            Count = count;
            LastMessageAt = lastMessageAt;
            FirstMessageAt = firstMessageAt;
        }
    }
}
