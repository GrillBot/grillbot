using System.Collections.Generic;
using GrillBot.Core.Managers.Performance;

namespace GrillBot.Data.Models.API.Statistics;

public class OperationStats
{
    public List<OperationStatItem> Statistics { get; set; } = new();
    public Dictionary<string, long> CountChartData { get; set; } = new();
    public Dictionary<string, long> TimeChartData { get; set; } = new();
}
