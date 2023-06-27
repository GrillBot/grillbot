using System;
using System.Collections.Generic;
using Discord;
using GrillBot.Core.Managers.Performance;

namespace GrillBot.Data.Models.API.System;

public class DashboardInfo
{
    public bool IsDevelopment { get; set; }
    public DateTime StartAt { get; set; }
    public long Uptime { get; set; }
    public long CpuTime { get; set; }
    public ConnectionState ConnectionState { get; set; }
    public long UsedMemory { get; set; }
    public bool IsActive { get; set; }
    public DateTime CurrentDateTime { get; set; }

    public Dictionary<string, int> ActiveOperations { get; set; } = new();
    public List<CounterStats> OperationStats { get; set; } = new();
}
