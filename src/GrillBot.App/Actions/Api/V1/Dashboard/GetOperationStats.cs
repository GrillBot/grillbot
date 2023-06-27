using GrillBot.Common.Models;
using GrillBot.Core.Managers.Performance;

namespace GrillBot.App.Actions.Api.V1.Dashboard;

public class GetOperationStats : ApiAction
{
    private ICounterManager CounterManager { get; }
    
    public GetOperationStats(ApiRequestContext apiContext, ICounterManager counterManager) : base(apiContext)
    {
        CounterManager = counterManager;
    }
    
    public List<CounterStats> Process()
    {
        var statistics = CounterManager.GetStatistics();

        return statistics
            .Select(o => new { Key = o.Section.Split('.'), Item = o })
            .GroupBy(o => o.Key.Length == 1 ? o.Key[0] : string.Join(".", o.Key.Take(o.Key.Length - 1)))
            .Select(o => new CounterStats
            {
                Count = o.Sum(x => x.Item.Count),
                Section = o.Key,
                TotalTime = o.Sum(x => x.Item.TotalTime)
            }).ToList();
    }
}
