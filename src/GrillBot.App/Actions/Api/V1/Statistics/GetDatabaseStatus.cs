using GrillBot.Cache.Services;
using GrillBot.Common.Models;
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

    public async Task<DatabaseStatistics> ProcessAsync()
    {
        var result = new DatabaseStatistics();

        await using var database = DatabaseBuilder.CreateRepository();
        result.Database = await database.Statistics.GetTablesStatusAsync();

        await using var cache = CacheBuilder.CreateRepository();
        result.Cache = await cache.StatisticsRepository.GetTableStatisticsAsync();

        return result;
    }
}
