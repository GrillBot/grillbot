using GrillBot.Data.Models.API.Users;
using System.Collections.Generic;

namespace GrillBot.Data.Models.API.Channels;

public class ChannelDetail : GuildChannelListItem
{
    public User LastMessageFrom { get; set; }
    public User MostActiveUser { get; set; }
    public Channel ParentChannel { get; set; }
    public List<Channel> Threads { get; set; }
}
