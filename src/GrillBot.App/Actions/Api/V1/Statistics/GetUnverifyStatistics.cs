using GrillBot.Common.Models;
using GrillBot.Database.Enums;

namespace GrillBot.App.Actions.Api.V1.Statistics;

public class GetUnverifyStatistics : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public GetUnverifyStatistics(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task<Dictionary<string, int>> ProcessByOperationAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        var statistics = await repository.Unverify.GetStatisticsByTypeAsync();

        return Enum.GetValues<UnverifyOperation>()
            .Select(o => new { Key = o.ToString(), Value = statistics.TryGetValue(o, out var val) ? val : 0 })
            .OrderByDescending(o => o.Value).ThenBy(o => o.Key)
            .ToDictionary(o => o.Key, o => o.Value);
    }

    public async Task<Dictionary<string, int>> ProcessByDateAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        return await repository.Unverify.GetStatisticsByDateAsync();
    }
}
