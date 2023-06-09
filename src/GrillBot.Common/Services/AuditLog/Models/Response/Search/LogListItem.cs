using GrillBot.Common.Services.AuditLog.Enums;

namespace GrillBot.Common.Services.AuditLog.Models.Response.Search;

public class LogListItem
{
    public string? GuildId { get; set; }
    public string? UserId { get; set; }
    public string? ChannelId { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid Id { get; set; }
    public LogType Type { get; set; }
    public bool IsDetailAvailable { get; set; }
    
    public List<File> Files { get; set; } = new();
    public object? Preview { get; set; }
}
