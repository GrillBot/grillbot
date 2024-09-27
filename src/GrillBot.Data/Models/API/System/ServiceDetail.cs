using GrillBot.Core.Managers.Performance;
using GrillBot.Core.Services.Diagnostics.Models;
using System;
using System.Collections.Generic;

namespace GrillBot.Data.Models.API.System;

public class ServiceDetail
{
    public string Name { get; set; } = null!;
    public string Url { get; set; } = null!;
    public string? ApiErrorMessage { get; set; }

    public long UsedMemory { get; set; }
    public long Uptime { get; set; }
    public long CpuTime { get; set; }
    public long RequestsCount { get; set; }
    public DateTime MeasuredFrom { get; set; }
    public List<RequestStatistics> Endpoints { get; set; } = new();
    public Dictionary<string, long>? DatabaseStatistics { get; set; }
    public List<OperationStatItem> Operations { get; set; } = new();
    public Dictionary<string, string> AdditionalData { get; set; } = new();
}
