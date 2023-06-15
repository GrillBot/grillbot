namespace GrillBot.Common.Services.AuditLog.Models.Request.CreateItems;

public class FileRequest
{
    public string Filename { get; set; } = null!;
    public string? Extension { get; set; }
    public long Size { get; set; }
}
