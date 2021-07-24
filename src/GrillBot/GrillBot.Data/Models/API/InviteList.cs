using Discord.WebSocket;
using GrillBot.Data.Models.API.Guilds;
using System.Collections.Generic;

namespace GrillBot.Data.Models.API
{
    public class InviteList
    {
        public Guild Guild { get; set; }
        public List<Invite> Invites { get; set; }

        public InviteList(SocketGuild guild)
        {
            Guild = new Guild(guild);
            Invites = new List<Invite>();
        }
    }
}
