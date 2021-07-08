using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GrillBot.Data.Models.AuditLog
{
    public class MemberUpdatedData
    {
        public Diff<string> Nickname { get; set; }
        public Diff<bool> IsMuted { get; set; }
        public Diff<bool> IsDeaf { get; set; }
        public List<AuditRoleUpdateInfo> Roles { get; set; }

        public MemberUpdatedData() { }

        public MemberUpdatedData(Diff<string> nickname, Diff<bool> muted, Diff<bool> deaf)
        {
            Nickname = nickname;
            IsMuted = muted;
            IsDeaf = deaf;
        }

        public MemberUpdatedData(IGuildUser before, IGuildUser after)
            : this(
                  new Diff<string>(before.Nickname, after.Nickname),
                  new Diff<bool>(before.IsMuted, after.IsMuted),
                  new Diff<bool>(before.IsDeafened, after.IsDeafened)
            )
        { }

        public MemberUpdatedData(MemberRoleAuditLogData data, SocketGuild guild)
        {
            Roles = data.Roles.Select(o =>
            {
                var role = guild.GetRole(o.RoleId);
                return role != null ? new AuditRoleUpdateInfo(role, o.Added) : null;
            }).Where(o => o != null).ToList();
        }

        [OnSerializing]
        internal void OnSerializing(StreamingContext _)
        {
            if (Nickname?.IsEmpty == true) Nickname = null;
            if (IsMuted?.IsEmpty == true) IsMuted = null;
            if (IsDeaf?.IsEmpty == true) IsDeaf = null;
            if (Roles?.Count == 0) Roles = null;
        }
    }
}
