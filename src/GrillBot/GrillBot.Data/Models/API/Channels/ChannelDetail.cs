using GrillBot.Data.Models.API.Users;
using System.Linq;

namespace GrillBot.Data.Models.API.Channels
{
    public class ChannelDetail : GuildChannel
    {
        public User LastMessageFrom { get; set; }
        public User MostActiveUser { get; set; }
        public Channel ParentChannel { get; set; }
        public long Flags { get; set; }

        public ChannelDetail() { }

        public ChannelDetail(Database.Entity.GuildChannel channel, int cachedMessagesCount) : base(channel, cachedMessagesCount)
        {
            ParentChannel = channel.ParentChannel != null ? new Channel(channel.ParentChannel) : null;
            Flags = channel.Flags;

            var lastMessageFrom = channel.Users.OrderByDescending(o => o.LastMessageAt).Select(o => o.User.User).FirstOrDefault();
            LastMessageFrom = lastMessageFrom == null ? null : new(lastMessageFrom);

            var mostActiveUser = channel.Users.OrderByDescending(o => o.Count).Select(o => o.User.User).FirstOrDefault();
            MostActiveUser = mostActiveUser == null ? null : new(mostActiveUser);
        }
    }
}
