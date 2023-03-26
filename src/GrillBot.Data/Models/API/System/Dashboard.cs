using System;
using System.Collections.Generic;
using Discord;
using GrillBot.Core.Managers.Performance;

namespace GrillBot.Data.Models.API.System;

public class Dashboard
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
    public Dictionary<string, long>? TodayAvgTimes { get; set; } = new();
    public List<DashboardApiCall>? InternalApiRequests { get; set; } = new();
    public List<DashboardApiCall>? PublicApiRequests { get; set; } = new();
    public List<DashboardJob>? Jobs { get; set; } = new();
    public List<DashboardCommand>? Commands { get; set; } = new();
    public List<DashboardService> Services { get; set; } = new();
}
