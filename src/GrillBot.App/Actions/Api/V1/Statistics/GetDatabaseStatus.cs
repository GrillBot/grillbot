using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Data.Models.API.Statistics;

namespace GrillBot.App.Actions.Api.V1.Statistics;

public class GetDatabaseStatus : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public GetDatabaseStatus(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var result = new DatabaseStatistics();

        using var database = DatabaseBuilder.CreateRepository();
        result.Database = await database.Statistics.GetTablesStatusAsync();
        result.Database = result.Database.OrderByDescending(o => o.Value).ToDictionary(o => o.Key, o => o.Value);

        return ApiResult.Ok(result);
    }
}
