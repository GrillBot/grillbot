using System;

namespace GrillBot.Data.Models.API.Channels
{
    public class ChannelUserStatItem
    {
        public int Position { get; set; }
        public string Username { get; set; }
        public string Nickname { get; set; }
        public string UserId { get; set; }
        public long Count { get; set; }
        public DateTime LastMessageAt { get; set; }
        public DateTime FirstMessageAt { get; set; }
    }
}
