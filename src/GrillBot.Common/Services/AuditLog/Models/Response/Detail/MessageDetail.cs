namespace GrillBot.Common.Services.AuditLog.Models.Response.Detail;

public class MessageDetail
{
    public string Text { get; set; } = null!;
    public string SourceAppName { get; set; } = null!;
    public string Source { get; set; } = null!;
}
