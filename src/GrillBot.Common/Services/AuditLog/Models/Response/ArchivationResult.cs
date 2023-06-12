namespace GrillBot.Common.Services.AuditLog.Models.Response;

public class ArchivationResult
{
    public string Xml { get; set; } = null!;
    public List<string> Files { get; set; } = new();
    public List<string> UserIds { get; set; } = new();
    public List<string> GuildIds { get; set; } = new();
    public List<string> ChannelIds { get; set; } = new();
    public int ItemsCount { get; set; }
    public long TotalFilesSize { get; set; }
}
