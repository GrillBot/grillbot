using Discord;

namespace GrillBot.Common.Services.AuditLog.Models.Request.CreateItems;

public class LogMessageRequest
{
    public string Message { get; set; } = null!;
    public LogSeverity Severity { get; set; }
    public string SourceAppName { get; set; } = null!;
    public string Source { get; set; } = null!;
}
