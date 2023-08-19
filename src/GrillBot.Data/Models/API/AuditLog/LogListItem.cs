using System;
using System.Collections.Generic;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Data.Models.API.Channels;
using GrillBot.Data.Models.API.Guilds;
using GrillBot.Data.Models.API.Users;

namespace GrillBot.Data.Models.API.AuditLog;

public class LogListItem
{
    public Guild? Guild { get; set; }
    public User? User { get; set; }
    public Channel? Channel { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid Id { get; set; }
    public LogType Type { get; set; }
    public bool IsDetailAvailable { get; set; }
    public List<File> Files { get; set; } = new();
    public object? Preview { get; set; }
}
