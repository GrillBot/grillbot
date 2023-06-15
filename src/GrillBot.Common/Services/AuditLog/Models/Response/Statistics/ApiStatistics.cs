namespace GrillBot.Common.Services.AuditLog.Models.Response.Statistics;

public class ApiStatistics
{
    public Dictionary<string, int> ByDateInternalApi { get; set; } = new();
    public Dictionary<string, int> ByDatePublicApi { get; set; } = new();
    public Dictionary<string, int> ByStatusCodeInternalApi { get; set; } = new();
    public Dictionary<string, int> ByStatusCodePublicApi { get; set; } = new();
    public List<StatisticItem> Endpoints { get; set; } = new();
}
