namespace GrillBot.Common.Managers.Counters;

public sealed class CounterItem : IDisposable
{
    public string Section { get; }
    public Guid Id { get; }

    private CounterManager CounterManager { get; }

    public CounterItem(CounterManager counterManager, string section)
    {
        Id = Guid.NewGuid();
        CounterManager = counterManager;
        Section = section;
    }

    public void Dispose()
    {
        CounterManager.Complete(this);
    }
}
