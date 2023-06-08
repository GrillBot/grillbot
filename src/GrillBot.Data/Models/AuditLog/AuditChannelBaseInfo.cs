using System;
using System.Collections.Generic;

namespace GrillBot.Data.Models.AuditLog;

public class AuditChannelBaseInfo : IComparable
{
    public ulong Id { get; set; }
    public string ChannelId { get; set; }

    public string Name { get; set; }
    public int? SlowMode { get; set; }

    public int CompareTo(object obj)
    {
        if (obj is not AuditChannelBaseInfo channel) return 1;

        var currentChannelId = !string.IsNullOrEmpty(ChannelId) ? ChannelId : Id.ToString();
        var otherChannelId = !string.IsNullOrEmpty(channel.ChannelId) ? channel.ChannelId : channel.Id.ToString();
        return otherChannelId == currentChannelId ? 0 : 1;
    }

    public override bool Equals(object obj) => CompareTo(obj) == 0;
    public override int GetHashCode() => (string.IsNullOrEmpty(ChannelId) ? Id.ToString() : ChannelId).GetHashCode();
    public static bool operator ==(AuditChannelBaseInfo left, AuditChannelBaseInfo right) => EqualityComparer<AuditChannelBaseInfo>.Default.Equals(left, right);
    public static bool operator !=(AuditChannelBaseInfo left, AuditChannelBaseInfo right) => !(left == right);
    public static bool operator >(AuditChannelBaseInfo left, AuditChannelBaseInfo right) => left.CompareTo(right) != 0;
    public static bool operator <(AuditChannelBaseInfo left, AuditChannelBaseInfo right) => left.CompareTo(right) != 0;
    public static bool operator <=(AuditChannelBaseInfo left, AuditChannelBaseInfo right) => left.CompareTo(right) != 0;
    public static bool operator >=(AuditChannelBaseInfo left, AuditChannelBaseInfo right) => left.CompareTo(right) != 0;

    public string GetId()
        => Id > 0 ? Id.ToString() : ChannelId;
}
