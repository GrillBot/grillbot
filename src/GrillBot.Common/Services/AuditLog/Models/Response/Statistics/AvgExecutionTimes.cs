namespace GrillBot.Common.Services.AuditLog.Models.Response.Statistics;

public class AvgExecutionTimes
{
    public Dictionary<string, double> InternalApi { get; set; } = new();
    public Dictionary<string, double> ExternalApi { get; set; } = new();
    public Dictionary<string, double> Jobs { get; set; } = new();
    public Dictionary<string, double> Interactions { get; set; } = new();
}
