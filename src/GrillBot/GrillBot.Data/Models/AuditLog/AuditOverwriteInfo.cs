using Discord;

namespace GrillBot.Data.Models.AuditLog;

public class AuditOverwriteInfo
{
    public PermissionTarget Target { get; set; }
    public ulong TargetId { get; set; }
    public ulong AllowValue { get; set; }
    public ulong DenyValue { get; set; }

    public AuditOverwriteInfo() { }

    public AuditOverwriteInfo(PermissionTarget target, ulong targetId, ulong allowValue, ulong denyValue)
    {
        Target = target;
        TargetId = targetId;
        AllowValue = allowValue;
        DenyValue = denyValue;
    }

    public AuditOverwriteInfo(Overwrite overwrite)
        : this(overwrite.TargetType, overwrite.TargetId, overwrite.Permissions.AllowValue, overwrite.Permissions.DenyValue) { }
}
