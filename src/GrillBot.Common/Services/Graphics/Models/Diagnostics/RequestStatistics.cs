namespace GrillBot.Common.Services.Graphics.Models.Diagnostics;

public class RequestStatistics
{
    public string Endpoint { get; set; } = null!;
    public long Count { get; set; }
    public DateTime LastRequestAt { get; set; }
}
