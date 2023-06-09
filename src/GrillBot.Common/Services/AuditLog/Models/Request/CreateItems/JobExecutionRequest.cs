namespace GrillBot.Common.Services.AuditLog.Models.Request.CreateItems;

public class JobExecutionRequest
{
    public string JobName { get; set; } = null!;
    public string Result { get; set; } = null!;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public bool WasError { get; set; }
    public string? StartUserId { get; set; }
}
