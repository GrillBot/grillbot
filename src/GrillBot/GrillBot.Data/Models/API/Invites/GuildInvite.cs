using GrillBot.Data.Models.API.Guilds;

namespace GrillBot.Data.Models.API.Invites
{
    public class GuildInvite : Invite
    {
        public Guild Guild { get; set; }

        public GuildInvite() { }

        public GuildInvite(Database.Entity.Invite entity) : base(entity)
        {
            Guild = entity.Guild != null ? new Guild(entity.Guild) : null;
        }
    }
}
