namespace GrillBot.Common.Services.Graphics.Models.Diagnostics;

public class RequestStatistics
{
    public string Endpoint { get; set; } = null!;
    public long Count { get; set; }
    public DateTime LastRequestAt { get; set; }
    public int TotalTime { get; set; }
    public int AvgTime { get; set; }
    public int LastTime { get; set; }
}
