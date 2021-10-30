using GrillBot.Data.Models.API.Users;
using System;

namespace GrillBot.Data.Models.API.Channels
{
    public class ChannelDetail : GuildChannel
    {
        public long MessagesCount { get; set; }
        public DateTime? FirstMessageAt { get; set; }
        public DateTime? LastMessageAt { get; set; }
        public User LastMessageFrom { get; set; }
        public User MostActiveUser { get; set; }

        public ChannelDetail() { }

        public ChannelDetail(Database.Entity.GuildChannel channel) : base(channel) { }
    }
}
