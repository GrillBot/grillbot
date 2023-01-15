namespace GrillBot.Common.Services.Graphics.Models.Diagnostics;

public class Stats
{
    public int TotalRequestCount { get; set; }
    public DailyStats TodayRequests { get; set; } = null!;
    public int ChartRequestsCount { get; set; }
    public DateTime MeasuredFrom { get; set; }
}
