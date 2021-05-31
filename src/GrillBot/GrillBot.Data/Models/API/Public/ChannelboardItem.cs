using Discord;
using System;

namespace GrillBot.Data.Models.API.Public
{
    public class ChannelboardItem
    {
        public string ChannelName { get; set; }
        public long Count { get; set; }
        public DateTime LastMessageAt { get; set; }

        public ChannelboardItem() { }

        public ChannelboardItem(IChannel channel, long count, DateTime lastMessageAt)
        {
            ChannelName = channel.Name;
            Count = count;
            LastMessageAt = lastMessageAt;
        }
    }
}
