using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Managers.Performance;

namespace GrillBot.App.Actions.Api.V3.Dashboard;

public class GetTopHeavyOperationsActions : ApiAction
{
    private readonly ICounterManager _counterManager;

    public GetTopHeavyOperationsActions(ApiRequestContext apiContext, ICounterManager counterManager) : base(apiContext)
    {
        _counterManager = counterManager;
    }

    public override Task<ApiResult> ProcessAsync()
    {
        var statistics = _counterManager.GetStatistics();

        var result = statistics
            .OrderByDescending(o => o.AverageTime)
            .ThenByDescending(o => o.Count)
            .ThenBy(o => o.Section)
            .Take(10)
            .ToList();

        return Task.FromResult(ApiResult.Ok(result));
    }
}
