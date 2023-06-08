using Discord;

namespace GrillBot.Common.Services.AuditLog.Models;

public class ThreadInfoRequest
{
    public string ThreadName { get; set; } = null!;
    public int? SlowMode { get; set; }
    public ThreadType Type { get; set; }
    public bool IsArchived { get; set; }
    public int ArchiveDuration { get; set; }
    public bool IsLocked { get; set; }
    public List<string> Tags { get; set; } = new();
    public string ParentChannelId { get; set; } = null!;
}
