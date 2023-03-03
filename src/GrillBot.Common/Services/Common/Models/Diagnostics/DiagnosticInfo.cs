namespace GrillBot.Common.Services.Common.Models.Diagnostics;

public class DiagnosticInfo
{
    public long UsedMemory { get; set; }
    public long Uptime { get; set; }
    public string Version { get; set; } = null!;
    public long RequestsCount { get; set; }
    public DateTime MeasuredFrom { get; set; }
    public List<RequestStatistic> Endpoints { get; set; } = new();
}
