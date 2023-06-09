namespace GrillBot.Common.Services.AuditLog.Models.Response.Search;

public class JobPreview
{
    public string JobName { get; set; } = null!;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public bool WasError { get; set; }
}
