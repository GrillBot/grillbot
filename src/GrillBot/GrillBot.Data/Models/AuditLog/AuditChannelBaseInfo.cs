using System;
using System.Collections.Generic;

namespace GrillBot.Data.Models.AuditLog;

public class AuditChannelBaseInfo : IComparable
{
    public ulong Id { get; set; }
    public string Name { get; set; }
    public int? SlowMode { get; set; }

    public AuditChannelBaseInfo() { }

    public AuditChannelBaseInfo(ulong id, string name, int? slowMode)
    {
        Id = id;
        Name = name;
        SlowMode = slowMode;
    }

    public int CompareTo(object obj) => obj is AuditChannelBaseInfo channel && channel.Id == Id ? 0 : 1;
    public override bool Equals(object obj) => CompareTo(obj) == 0;
    public override int GetHashCode() => Id.ToString().GetHashCode();
    public static bool operator ==(AuditChannelBaseInfo left, AuditChannelBaseInfo right) => EqualityComparer<AuditChannelBaseInfo>.Default.Equals(left, right);
    public static bool operator !=(AuditChannelBaseInfo left, AuditChannelBaseInfo right) => !(left == right);
    public static bool operator >(AuditChannelBaseInfo left, AuditChannelBaseInfo right) => left.CompareTo(right) != 0;
    public static bool operator <(AuditChannelBaseInfo left, AuditChannelBaseInfo right) => left.CompareTo(right) != 0;
    public static bool operator <=(AuditChannelBaseInfo left, AuditChannelBaseInfo right) => left.CompareTo(right) != 0;
    public static bool operator >=(AuditChannelBaseInfo left, AuditChannelBaseInfo right) => left.CompareTo(right) != 0;
}
