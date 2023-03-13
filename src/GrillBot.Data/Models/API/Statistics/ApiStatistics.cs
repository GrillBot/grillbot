using System.Collections.Generic;

namespace GrillBot.Data.Models.API.Statistics;

public class ApiStatistics
{
    /// <summary>
    /// Statistics by date of internal API.
    /// </summary>
    public Dictionary<string, int> ByDateInternalApi { get; set; } = new();

    /// <summary>
    /// Statistics by date of public API.
    /// </summary>
    public Dictionary<string, int> ByDatePublicApi { get; set; } = new();

    /// <summary>
    /// Statistics by status code of internal API.
    /// </summary>
    public Dictionary<string, int> ByStatusCodeInternalApi { get; set; } = new();

    /// <summary>
    /// Statistics by status code of public API.
    /// </summary>
    public Dictionary<string, int> ByStatusCodePublicApi { get; set; } = new();

    /// <summary>
    /// Statistics by endpoints.
    /// </summary>
    public List<StatisticItem> Endpoints { get; set; } = new();
}
