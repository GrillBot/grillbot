using Discord.WebSocket;
using GrillBot.Data.Models.API.Guilds;
using GrillBot.Data.Models.API.Users;
using System.Collections.Generic;

namespace GrillBot.Data.Models.API.Public
{
    public class Channelboard
    {
        public string SessionId { get; set; }
        public Guild Guild { get; set; }
        public User User { get; set; }
        public List<ChannelboardItem> Channels { get; set; }

        public Channelboard() { }

        public Channelboard(string sessionId, SocketGuild guild, SocketUser user)
        {
            SessionId = sessionId;
            Guild = new Guild(guild);
            User = new User(user);
            Channels = new List<ChannelboardItem>();
        }
    }
}
