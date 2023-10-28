using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Managers.Performance;
using GrillBot.Data.Models.API.Statistics;

namespace GrillBot.App.Actions.Api.V1.Statistics;

public class GetOperationStats : ApiAction
{
    private ICounterManager CounterManager { get; }

    public GetOperationStats(ApiRequestContext apiContext, ICounterManager counterManager) : base(apiContext)
    {
        CounterManager = counterManager;
    }

    public override Task<ApiResult> ProcessAsync()
    {
        var statistics = CounterManager.GetStatistics();
        var result = new OperationStats
        {
            CountChartData = statistics.ToDictionary(o => o.Section, o => o.Count),
            TimeChartData = statistics.ToDictionary(o => o.Section, o => o.AverageTime),
            Statistics = OperationCounterConverter.ComputeTree(statistics)
        };

        return Task.FromResult(ApiResult.Ok(result));
    }
}
