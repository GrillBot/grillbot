using System;
using System.Collections.Generic;
using Discord;
using GrillBot.Common.Managers.Counters;

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
    
    public Dictionary<string, int> ActiveOperations { get; set; }
    public List<CounterStats> OperationStats { get; set; }
    public Dictionary<string, long> TodayAvgTimes { get; set; }
    public List<DashboardApiCall> InternalApiRequests { get; set; }
    public List<DashboardApiCall> PublicApiRequests { get; set; }
    public List<DashboardJob> Jobs { get; set; }
    public List<DashboardCommand> Commands { get; set; }
    public DashboardServices? Services { get; set; }
}
