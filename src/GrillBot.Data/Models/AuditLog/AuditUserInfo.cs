using Discord;
using System;

namespace GrillBot.Data.Models.AuditLog;

public class AuditUserInfo : IComparable
{
    public ulong Id { get; set; }
    public string UserId { get; set; }

    public string Username { get; set; }
    public string Discriminator { get; set; }

    public AuditUserInfo()
    {
    }

    public AuditUserInfo(IUser user)
    {
        UserId = user.Id.ToString();
        Username = user.Username;
        Discriminator = user.Discriminator;
    }

    public override string ToString() => string.IsNullOrEmpty(Discriminator) ? Username : $"{Username}#{Discriminator}";

    public int CompareTo(object obj)
        => obj is AuditUserInfo user && (user.Id == Id || user.UserId == UserId) ? 0 : 1;

    public override bool Equals(object obj)
        => obj is AuditUserInfo info && (Id == info.Id || UserId == info.UserId);

    public override int GetHashCode()
        => (!string.IsNullOrEmpty(UserId) ? UserId : Id.ToString()).GetHashCode();

    public static bool operator ==(AuditUserInfo left, AuditUserInfo right) => left != null && left.CompareTo(right) == 0;
    public static bool operator !=(AuditUserInfo left, AuditUserInfo right) => left != null && left.CompareTo(right) != 0;
    public static bool operator >(AuditUserInfo left, AuditUserInfo right) => left.CompareTo(right) != 0;
    public static bool operator <(AuditUserInfo left, AuditUserInfo right) => left.CompareTo(right) != 0;
    public static bool operator <=(AuditUserInfo left, AuditUserInfo right) => left.CompareTo(right) != 0;
    public static bool operator >=(AuditUserInfo left, AuditUserInfo right) => left.CompareTo(right) != 0;

    public string GetId()
        => Id > 0 ? Id.ToString() : UserId;
}
