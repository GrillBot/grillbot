namespace GrillBot.Common.Managers.Counters;

public sealed class CounterItem : IDisposable
{
    public string Section { get; }
    public Guid Id { get; }
    public DateTime StartAt { get; set; }

    private CounterManager CounterManager { get; }

    public CounterItem(CounterManager counterManager, string section)
    {
        Id = Guid.NewGuid();
        StartAt = DateTime.Now;
        CounterManager = counterManager;
        Section = section;
    }

    public void Dispose()
    {
        CounterManager.Complete(this);
    }
}
