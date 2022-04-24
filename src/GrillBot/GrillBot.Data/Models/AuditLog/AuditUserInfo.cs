using Discord;
using GrillBot.Data.Extensions;
using System;

namespace GrillBot.Data.Models.AuditLog;

public class AuditUserInfo : IComparable
{
    public ulong Id { get; set; }
    public string Username { get; set; }
    public string Discriminator { get; set; }

    public AuditUserInfo() { }

    public AuditUserInfo(IUser user)
    {
        Id = user.Id;
        Username = user.Username;
        Discriminator = user.Discriminator;
    }

    public AuditUserInfo(Database.Entity.User user)
    {
        Id = user.Id.ToUlong();
        Username = user.Username;
        Discriminator = user.Discriminator;
    }

    public override string ToString() => string.IsNullOrEmpty(Discriminator) ? Username : $"{Username}#{Discriminator}";

    public int CompareTo(object obj)
    {
        return obj is AuditUserInfo user && user.Id == Id ? 0 : 1;
    }

    public override bool Equals(object obj)
    {
        return obj is AuditUserInfo info && Id == info.Id;
    }

    public override int GetHashCode()
    {
        return Id.ToString().GetHashCode();
    }

    public static bool operator ==(AuditUserInfo left, AuditUserInfo right) => left.CompareTo(right) == 0;
    public static bool operator !=(AuditUserInfo left, AuditUserInfo right) => left.CompareTo(right) != 0;
    public static bool operator >(AuditUserInfo left, AuditUserInfo right) => left.CompareTo(right) != 0;
    public static bool operator <(AuditUserInfo left, AuditUserInfo right) => left.CompareTo(right) != 0;
    public static bool operator <=(AuditUserInfo left, AuditUserInfo right) => left.CompareTo(right) != 0;
    public static bool operator >=(AuditUserInfo left, AuditUserInfo right) => left.CompareTo(right) != 0;
}
