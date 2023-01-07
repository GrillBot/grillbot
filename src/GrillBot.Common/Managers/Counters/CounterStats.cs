namespace GrillBot.Common.Managers.Counters;

public class CounterStats
{
    public string Section { get; set; } = null!;
    public long TotalTime { get; set; }
    public long Count { get; set; }

    public long AverageTime => TotalTime / Count;

    public void Increment(double duration)
    {
        Count++;
        TotalTime += (long)duration;
    }
}
