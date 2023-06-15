using System.Collections.Generic;
using System.Runtime.Serialization;
using Discord;

namespace GrillBot.Data.Models.AuditLog;

public class AuditThreadInfo : AuditChannelBaseInfo
{
    public ThreadType ThreadType { get; set; }
    public bool IsArchived { get; set; }
    public ThreadArchiveDuration ArchiveDuration { get; set; }
    public bool IsLocked { get; set; }
    public List<string>? Tags { get; set; }

    [OnSerializing]
    internal void OnSerializing(StreamingContext _)
    {
        if (Tags is { Count: 0 }) Tags = null;
    }
}
