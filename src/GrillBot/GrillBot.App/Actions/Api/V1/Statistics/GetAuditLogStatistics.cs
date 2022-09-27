using GrillBot.Common.Models;
using GrillBot.Database.Enums;

namespace GrillBot.App.Actions.Api.V1.Statistics;

public class GetAuditLogStatistics : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public GetAuditLogStatistics(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task<Dictionary<string, int>> ProcessByTypeAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        var data = await repository.AuditLog.GetStatisticsByTypeAsync();

        return Enum.GetValues<AuditLogItemType>()
            .Where(o => o != AuditLogItemType.None)
            .Select(o => new { Key = o.ToString(), Value = data.TryGetValue(o, out var val) ? val : 0 })
            .OrderByDescending(o => o.Value).ThenBy(o => o.Key)
            .ToDictionary(o => o.Key, o => o.Value);
    }

    public async Task<Dictionary<string, int>> ProcessByDateAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        return await repository.AuditLog.GetStatisticsByDateAsync();
    }
}
