using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;

namespace GrillBot.App.Actions.Api.V1.Searching;

public class RemoveSearches : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public RemoveSearches(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var ids = (long[])Parameters[0]!;
        await using var repository = DatabaseBuilder.CreateRepository();

        var searches = await repository.Searching.FindSearchesByIdsAsync(ids);
        if (searches.Count == 0)
            return ApiResult.Ok();

        repository.RemoveCollection(searches);
        await repository.CommitAsync();
        return ApiResult.Ok();
    }
}
