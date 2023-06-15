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
}
