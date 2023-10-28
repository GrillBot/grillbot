using GrillBot.Cache.Services;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Data.Models.API.Statistics;

namespace GrillBot.App.Actions.Api.V1.Statistics;

public class GetDatabaseStatus : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private GrillBotCacheBuilder CacheBuilder { get; }

    public GetDatabaseStatus(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, GrillBotCacheBuilder cacheBuilder) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        CacheBuilder = cacheBuilder;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var result = new DatabaseStatistics();

        await using var database = DatabaseBuilder.CreateRepository();
        result.Database = await database.Statistics.GetTablesStatusAsync();
        result.Database = result.Database.OrderByDescending(o => o.Value).ToDictionary(o => o.Key, o => o.Value);

        await using var cache = CacheBuilder.CreateRepository();
        result.Cache = await cache.StatisticsRepository.GetTableStatisticsAsync();
        result.Cache = result.Cache.OrderByDescending(o => o.Value).ToDictionary(o => o.Key, o => o.Value);

        return ApiResult.Ok(result);
    }
}
