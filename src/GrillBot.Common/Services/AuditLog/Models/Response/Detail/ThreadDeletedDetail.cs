using Discord;

namespace GrillBot.Common.Services.AuditLog.Models.Response.Detail;

public class ThreadDeletedDetail
{
    public string Name { get; set; } = null!;
    public int? SlowMode { get; set; }
    public ThreadType Type { get; set; }
    public bool IsArchived { get; set; }
    public ThreadArchiveDuration ArchivedDuration { get; set; }
    public bool IsLocked { get; set; }
}
