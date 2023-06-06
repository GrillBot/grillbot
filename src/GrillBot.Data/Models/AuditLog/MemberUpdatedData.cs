using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GrillBot.Core.Models;

namespace GrillBot.Data.Models.AuditLog;

public class MemberUpdatedData
{
    public AuditUserInfo Target { get; set; } = null!;
    public Diff<string>? Nickname { get; set; }
    public Diff<bool>? IsMuted { get; set; }
    public Diff<bool>? IsDeaf { get; set; }
    public List<AuditRoleUpdateInfo>? Roles { get; set; } = new();
    public Diff<TimeSpan?>? SelfUnverifyMinimalTime { get; set; }
    public Diff<int>? Flags { get; set; }
    public Diff<DateTime?>? TimeoutUntil { get; set; }

    public MemberUpdatedData()
    {
    }

    public MemberUpdatedData(AuditUserInfo target)
    {
        Target = target;
    }

    public MemberUpdatedData(Database.Entity.User before, Database.Entity.User after) : this(new AuditUserInfo(after))
    {
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
        if (SelfUnverifyMinimalTime?.IsEmpty() == true) SelfUnverifyMinimalTime = null;
        if (Flags?.IsEmpty() == true) Flags = null;
        if (TimeoutUntil?.IsEmpty() == true) TimeoutUntil = null;
    }
}
