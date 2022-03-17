using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GrillBot.Data.Models.AuditLog;

public class MemberUpdatedData
{
    public AuditUserInfo Target { get; set; }
    public Diff<string> Nickname { get; set; }
    public Diff<bool> IsMuted { get; set; }
    public Diff<bool> IsDeaf { get; set; }
    public List<AuditRoleUpdateInfo> Roles { get; set; }

    public MemberUpdatedData()
    {
        Roles = new List<AuditRoleUpdateInfo>();
    }

    public MemberUpdatedData(AuditUserInfo target) : this()
    {
        Target = target;
    }

    public MemberUpdatedData(AuditUserInfo target, Diff<string> nickname, Diff<bool> muted, Diff<bool> deaf) : this(target)
    {
        Nickname = nickname;
        IsMuted = muted;
        IsDeaf = deaf;
    }

    public MemberUpdatedData(IGuildUser before, IGuildUser after)
        : this(
              new AuditUserInfo(before),
              new Diff<string>(before.Nickname, after.Nickname),
              new Diff<bool>(before.IsMuted, after.IsMuted),
              new Diff<bool>(before.IsDeafened, after.IsDeafened)
        )
    { }

    [OnSerializing]
    internal void OnSerializing(StreamingContext _)
    {
        if (Nickname?.IsEmpty == true) Nickname = null;
        if (IsMuted?.IsEmpty == true) IsMuted = null;
        if (IsDeaf?.IsEmpty == true) IsDeaf = null;
        if (Roles?.Count == 0) Roles = null;
    }
}
