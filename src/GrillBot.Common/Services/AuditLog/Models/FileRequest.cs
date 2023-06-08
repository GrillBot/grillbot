namespace GrillBot.Common.Services.AuditLog.Models;

public class FileRequest
{
    public string Filename { get; set; } = null!;
    public string? Extension { get; set; }
    public long Size { get; set; }
}
