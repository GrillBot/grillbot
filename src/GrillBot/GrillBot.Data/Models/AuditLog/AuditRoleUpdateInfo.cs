using Discord;

namespace GrillBot.Data.Models.AuditLog;

public class AuditRoleUpdateInfo : AuditRoleInfo
{
    public bool Added { get; set; }

    public AuditRoleUpdateInfo() { }

    public AuditRoleUpdateInfo(ulong id, string name, Color color) : base(id, name, color)
    {
    }

    public AuditRoleUpdateInfo(IRole role, bool added) : base(role)
    {
        Added = added;
    }
}
