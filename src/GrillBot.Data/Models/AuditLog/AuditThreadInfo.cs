using Discord;
using Discord.Rest;

namespace GrillBot.Data.Models.AuditLog;

public class AuditThreadInfo : AuditChannelBaseInfo
{
    public ThreadType ThreadType { get; set; }
    public bool IsArchived { get; set; }
    public ThreadArchiveDuration ArchiveDuration { get; set; }
    public bool IsLocked { get; set; }

    public AuditThreadInfo() { }

    public AuditThreadInfo(ulong id, string name, ThreadType threadType, bool isArchived, ThreadArchiveDuration archiveDuration,
        bool isLocked, int? slowMode) : base(id, name, slowMode)
    {
        ThreadType = threadType;
        IsArchived = isArchived;
        ArchiveDuration = archiveDuration;
        IsLocked = isLocked;
    }

    public AuditThreadInfo(ThreadDeleteAuditLogData data)
        : this(data.ThreadId, data.ThreadName, data.ThreadType, data.IsArchived, data.AutoArchiveDuration, data.IsLocked, data.SlowModeInterval) { }

    public AuditThreadInfo(IThreadChannel thread) : this(thread.Id, thread.Name, thread.Type, thread.IsArchived, thread.AutoArchiveDuration,
        thread.IsLocked, thread.SlowModeInterval)
    { }
}
