using Discord;
using GrillBot.Core.Models;

namespace GrillBot.Common.Services.AuditLog.Models.Response.Detail;

public class OverwriteUpdatedDetail
{
    public string TargetId { get; set; } = null!;
    public PermissionTarget TargetType { get; set; }
    
    public Diff<List<string>>? Allow { get; set; }
    public Diff<List<string>>? Deny { get; set; }
}
