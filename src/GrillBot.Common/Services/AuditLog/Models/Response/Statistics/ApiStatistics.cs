namespace GrillBot.Common.Services.AuditLog.Models.Response.Statistics;

public class ApiStatistics
{
    public Dictionary<string, long> ByDateInternalApi { get; set; } = new();
    public Dictionary<string, long> ByDatePublicApi { get; set; } = new();
    public Dictionary<string, long> ByStatusCodeInternalApi { get; set; } = new();
    public Dictionary<string, long> ByStatusCodePublicApi { get; set; } = new();
    public List<StatisticItem> Endpoints { get; set; } = new();
}
