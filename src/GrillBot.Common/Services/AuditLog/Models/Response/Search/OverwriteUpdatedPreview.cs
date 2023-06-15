using Discord;

namespace GrillBot.Common.Services.AuditLog.Models.Response.Search;

public class OverwriteUpdatedPreview
{
    public string TargetId { get; set; } = null!;
    public PermissionTarget TargetType { get; set; }
}
