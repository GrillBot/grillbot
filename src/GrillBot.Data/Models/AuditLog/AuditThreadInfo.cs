﻿using System.Collections.Generic;
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

    public AuditThreadInfo()
    {
    }

    public AuditThreadInfo(ulong id, string name, ThreadType threadType, bool isArchived, ThreadArchiveDuration archiveDuration,
        bool isLocked, int? slowMode) : base(id, name, slowMode)
    {
        ThreadType = threadType;
        IsArchived = isArchived;
        ArchiveDuration = archiveDuration;
        IsLocked = isLocked;
        Tags = new List<string>();
    }

    public AuditThreadInfo(IThreadChannel thread) : this(thread.Id, thread.Name, thread.Type, thread.IsArchived, thread.AutoArchiveDuration,
        thread.IsLocked, thread.SlowModeInterval)
    {
    }

    [OnSerializing]
    internal void OnSerializing(StreamingContext _)
    {
        if (Tags is { Count: 0 }) Tags = null;
    }
}
