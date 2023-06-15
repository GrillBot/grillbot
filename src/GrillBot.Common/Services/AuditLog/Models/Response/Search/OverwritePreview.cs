using Discord;

namespace GrillBot.Common.Services.AuditLog.Models.Response.Search;

public class OverwritePreview
{
    public string TargetId { get; set; } = null!;
    public PermissionTarget TargetType { get; set; }

    public List<string> Allow { get; set; } = new();
    public List<string> Deny { get; set; } = new();
}
