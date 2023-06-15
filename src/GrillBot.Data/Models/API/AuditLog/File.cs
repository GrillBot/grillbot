namespace GrillBot.Data.Models.API.AuditLog;

public class File
{
    public string Filename { get; set; } = null!;
    public long Size { get; set; }
    public string Link { get; set; } = null!;
}
