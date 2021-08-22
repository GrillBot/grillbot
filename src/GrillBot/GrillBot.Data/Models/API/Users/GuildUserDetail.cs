using GrillBot.Data.Models.API.Channels;
using GrillBot.Data.Models.API.Guilds;
using GrillBot.Data.Models.API.Unverify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrillBot.Data.Models.API.Users
{
    public class GuildUserDetail
    {
        public Guild Guild { get; set; }
        public long Points { get; set; }
        public long GivenReactions { get; set; }
        public long ObtainedReactions { get; set; }
        public string Nickname { get; set; }
        public Invites.Invite UsedInvite { get; set; }
        public List<Invites.InviteBase> CreatedInvites { get; set; }
        public List<UserGuildChannel> Channels { get; set; }

        public GuildUserDetail() { }

        public GuildUserDetail(Database.Entity.GuildUser user)
        {
            Guild = new Guild(user.Guild);
            Points = user.Points;
            GivenReactions = user.GivenReactions;
            ObtainedReactions = user.ObtainedReactions;
            Nickname = user.Nickname;
            UsedInvite = user.UsedInvite == null ? null : new Invites.Invite(user.UsedInvite);
            CreatedInvites = user.CreatedInvites.Select(o => new Invites.InviteBase(o)).OrderByDescending(o => o.CreatedAt).ToList();
            Channels = user.Channels.Select(o => new UserGuildChannel(o)).OrderBy(o => o.Channel.Name).ToList();
        }
    }
}
