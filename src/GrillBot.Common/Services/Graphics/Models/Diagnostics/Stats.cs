using GrillBot.Common.Services.Common.Models.Diagnostics;

namespace GrillBot.Common.Services.Graphics.Models.Diagnostics;

public class Stats
{
    public int RequestsCount { get; set; }
    public DateTime MeasuredFrom { get; set; }
    public List<RequestStatistic> Endpoints { get; set; } = new();
}
