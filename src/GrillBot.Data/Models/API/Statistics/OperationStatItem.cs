using System.Collections.Generic;
using GrillBot.Common.Managers.Counters;

namespace GrillBot.Data.Models.API.Statistics;

public class OperationStatItem
{
    public string Section { get; set; } = null!;
    public long Count { get; set; }
    public long TotalTime { get; set; }

    public long AverageTime => Count == 0 ? 0 : TotalTime / Count;

    public List<OperationStatItem> ChildItems { get; set; } = new();
}
