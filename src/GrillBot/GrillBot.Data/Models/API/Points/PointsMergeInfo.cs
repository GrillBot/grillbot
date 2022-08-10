using System;

namespace GrillBot.Data.Models.API.Points;

public class PointsMergeInfo
{
    /// <summary>
    /// The start date of the migrated period. 
    /// </summary>
    public DateTime MergeRangeFrom { get; set; }

    /// <summary>
    /// The end date of the migrated period. If the period starts and ends at the same time, then this property will contain a null value.
    /// </summary>
    public DateTime? MergeRangeTo { get; set; }

    public int MergedItemsCount { get; set; }
}
