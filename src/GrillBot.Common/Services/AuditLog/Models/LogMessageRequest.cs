using Discord;

namespace GrillBot.Common.Services.AuditLog.Models;

public class LogMessageRequest
{
    public string Message { get; set; } = null!;
    public LogSeverity Severity { get; set; }
}
