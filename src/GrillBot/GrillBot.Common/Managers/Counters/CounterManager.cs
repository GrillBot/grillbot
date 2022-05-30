namespace GrillBot.Common.Managers.Counters;

public class CounterManager
{
    private List<CounterItem> ActiveCounters { get; } = new();
    private readonly object _lock = new();

    public CounterItem Create(string section)
    {
        lock (_lock)
        {
            var item = new CounterItem(this, section);

            ActiveCounters.Add(item);
            return item;
        }
    }

    public void Complete(CounterItem item)
    {
        lock (_lock)
        {
            ActiveCounters.RemoveAll(o => o.Section == item.Section && o.Id == item.Id);
        }
    }

    public Dictionary<string, int> GetActiveCounters()
    {
        lock (_lock)
        {
            return ActiveCounters
                .GroupBy(o => o.Section)
                .Select(o => new { o.Key, Count = o.Count() })
                .OrderByDescending(o => o.Count)
                .ThenBy(o => o.Key)
                .ToDictionary(o => o.Key, o => o.Count);
        }
    }
}
