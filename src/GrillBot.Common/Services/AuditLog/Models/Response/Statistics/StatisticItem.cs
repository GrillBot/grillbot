namespace GrillBot.Common.Services.AuditLog.Models.Response.Statistics;

public class StatisticItem
{
    public string Key { get; set; } = null!;
    public DateTime Last { get; set; }
    public long SuccessCount { get; set; }
    public long FailedCount { get; set; }
    public long MinDuration { get; set; }
    public long MaxDuration { get; set; }
    public long TotalDuration { get; set; }
    public long LastRunDuration { get; set; }
    public int SuccessRate { get; set; }
    public int AvgDuration { get; set; }
}
