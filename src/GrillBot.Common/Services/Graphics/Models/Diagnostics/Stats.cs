using GrillBot.Core.Services.Diagnostics.Models;

namespace GrillBot.Common.Services.Graphics.Models.Diagnostics;

public class Stats
{
    public int RequestsCount { get; set; }
    public DateTime MeasuredFrom { get; set; }
    public List<RequestStatistics> Endpoints { get; set; } = new();
    public long CpuTime { get; set; }
}
