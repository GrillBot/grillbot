namespace GrillBot.Common.Services.AuditLog.Models.Response.Statistics;

public class StatisticItem
{
    public string Key { get; set; } = null!;
    public DateTime Last { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public int MinDuration { get; set; }
    public int MaxDuration { get; set; }
    public int TotalDuration { get; set; }
    public int LastRunDuration { get; set; }
    public int SuccessRate { get; set; }
    public int AvgDuration { get; set; }
}
