namespace GrillBot.Common.Services.AuditLog.Models.Response.Info;

public class JobInfo
{
    public string Name { get; set; } = null!;
    public int StartCount { get; set; }
    public int? LastRunDuration { get; set; }
    public DateTime? LastStartAt { get; set; }
    public int FailedCount { get; set; }
    public int TotalDuration { get; set; }
    public int MinTime { get; set; }
    public int MaxTime { get; set; }
    public int AvgTime { get; set; }
}
