using GrillBot.Common.Models;

namespace GrillBot.App.Actions.Api.V1.Searching;

public class RemoveSearches : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public RemoveSearches(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task ProcessAsync(IEnumerable<long> ids)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var searches = await repository.Searching.FindSearchesByIdsAsync(ids);
        if (searches.Count == 0) return;

        repository.RemoveCollection(searches);
        await repository.CommitAsync();
    }
}
