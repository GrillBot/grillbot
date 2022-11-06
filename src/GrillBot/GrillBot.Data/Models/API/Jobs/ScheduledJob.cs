using System;

namespace GrillBot.Data.Models.API.Jobs;

public class ScheduledJob
{
    public string Name { get; set; }
    public int StartCount { get; set; }
    public int AverageTime { get; set; }
    public int MinTime { get; set; }
    public int MaxTime { get; set; }
    public int TotalTime { get; set; }
    public DateTime? LastRun { get; set; }
    public DateTime NextRun { get; set; }
    public bool Running { get; set; }
    public int? LastRunDuration { get; set; }
    public bool IsActive { get; set; }
    public int FailedCount { get; set; }
}
