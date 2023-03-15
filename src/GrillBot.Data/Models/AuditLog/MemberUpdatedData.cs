using Discord;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GrillBot.Core.Models;

namespace GrillBot.Data.Models.AuditLog;

public class MemberUpdatedData
{
    public AuditUserInfo Target { get; set; }
    public Diff<string>? Nickname { get; set; }
    public Diff<bool>? IsMuted { get; set; }
    public Diff<bool>? IsDeaf { get; set; }
    public List<AuditRoleUpdateInfo>? Roles { get; set; }
    public Diff<string?>? Note { get; set; }
    public Diff<TimeSpan?>? SelfUnverifyMinimalTime { get; set; }
    public Diff<int>? Flags { get; set; }

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
    {
    }

    public MemberUpdatedData(Database.Entity.User before, Database.Entity.User after)
    {
        Target = new AuditUserInfo(after);
        Note = new Diff<string?>(before.Note, after.Note);
        SelfUnverifyMinimalTime = new Diff<TimeSpan?>(before.SelfUnverifyMinimalTime, after.SelfUnverifyMinimalTime);
        Flags = new Diff<int>(before.Flags, after.Flags);
    }

    [OnSerializing]
    internal void OnSerializing(StreamingContext _)
    {
        if (Nickname?.IsEmpty() == true) Nickname = null;
        if (IsMuted?.IsEmpty() == true) IsMuted = null;
        if (IsDeaf?.IsEmpty() == true) IsDeaf = null;
        if (Roles?.Count == 0) Roles = null;
        if (Note?.IsEmpty() == true) Note = null;
        if (SelfUnverifyMinimalTime?.IsEmpty() == true) SelfUnverifyMinimalTime = null;
        if (Flags?.IsEmpty() == true) Flags = null;
    }
}
