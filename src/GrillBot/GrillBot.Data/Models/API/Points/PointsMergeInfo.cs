using System;

namespace GrillBot.Data.Models.API.Points;

public class PointsMergeInfo
{
    public DateTime MergeRangeFrom { get; set; }
    public DateTime MergeRangeTo { get; set; }
    public int MergedItemsCount { get; set; }
}
