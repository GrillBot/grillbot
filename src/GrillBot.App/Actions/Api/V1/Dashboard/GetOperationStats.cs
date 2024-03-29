﻿using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Managers.Performance;

namespace GrillBot.App.Actions.Api.V1.Dashboard;

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
        var result = statistics
            .Select(o => new { Key = o.Section.Split('.'), Item = o })
            .GroupBy(o => o.Key.Length == 1 ? o.Key[0] : string.Join(".", o.Key.Take(2)))
            .Select(o => new CounterStats
            {
                Count = o.Sum(x => x.Item.Count),
                Section = o.Key,
                TotalTime = o.Sum(x => x.Item.TotalTime)
            })
            .OrderBy(o => o.Section)
            .ToList();

        return Task.FromResult(ApiResult.Ok(result));
    }
}
