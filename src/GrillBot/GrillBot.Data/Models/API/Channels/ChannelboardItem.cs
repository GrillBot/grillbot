using GrillBot.Data.Models.API.Guilds;
using System;

namespace GrillBot.Data.Models.API.Channels
{
    public class ChannelboardItem
    {
        public GuildChannelListItem Channel { get; set; }
        public long Count { get; set; }
        public DateTime LastMessageAt { get; set; }
        public DateTime FirstMessageAt { get; set; }

        public ChannelboardItem() { }

        public ChannelboardItem(Database.Entity.GuildChannel channel, long count,
            DateTime lastMessageAt, DateTime firstMessageAt)
        {
            Channel = new GuildChannelListItem(channel);
            Count = count;
            LastMessageAt = lastMessageAt;
            FirstMessageAt = firstMessageAt;
        }
    }
}
