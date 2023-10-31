using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Managers.Performance;

namespace GrillBot.App.Actions.Api.V1.Dashboard;

public class GetActiveOperations : ApiAction
{
    private ICounterManager CounterManager { get; }

    public GetActiveOperations(ApiRequestContext apiContext, ICounterManager counterManager) : base(apiContext)
    {
        CounterManager = counterManager;
    }

    public override Task<ApiResult> ProcessAsync()
    {
        var result = CounterManager.GetActiveCounters();
        return Task.FromResult(ApiResult.Ok(result));
    }
}
