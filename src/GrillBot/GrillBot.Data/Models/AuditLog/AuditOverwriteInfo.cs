using Discord;
using Newtonsoft.Json;

namespace GrillBot.Data.Models.AuditLog;

public class AuditOverwriteInfo
{
    [JsonProperty("target")]
    public PermissionTarget TargetType { get; set; }

    public ulong TargetId { get; set; }
    public string TargetIdValue { get; set; }

    public ulong AllowValue { get; set; }
    public ulong DenyValue { get; set; }

    public AuditOverwriteInfo()
    {
    }

    public AuditOverwriteInfo(PermissionTarget targetType, ulong targetId, ulong allowValue, ulong denyValue)
    {
        TargetType = targetType;
        TargetIdValue = targetId.ToString();
        AllowValue = allowValue;
        DenyValue = denyValue;
    }

    public AuditOverwriteInfo(Overwrite overwrite)
        : this(overwrite.TargetType, overwrite.TargetId, overwrite.Permissions.AllowValue, overwrite.Permissions.DenyValue)
    {
    }
}
