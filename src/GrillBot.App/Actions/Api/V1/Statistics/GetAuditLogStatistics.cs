using GrillBot.Common.Models;
using GrillBot.Data.Models.API.Statistics;
using GrillBot.Database.Enums;
using GrillBot.Database.Services.Repository;

namespace GrillBot.App.Actions.Api.V1.Statistics;

public class GetAuditLogStatistics : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public GetAuditLogStatistics(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task<AuditLogStatistics> ProcessAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var statistics = new AuditLogStatistics
        {
            ByType = await ProcessByTypeAsync(repository),
            ByDate = await repository.AuditLog.GetStatisticsByDateAsync(),
        };

        await ProcessFileStatisticsAsync(statistics, repository);
        return statistics;
    }

    private static async Task<Dictionary<string, int>> ProcessByTypeAsync(GrillBotRepository repository)
    {
        var data = await repository.AuditLog.GetStatisticsByTypeAsync();

        return Enum.GetValues<AuditLogItemType>()
            .Where(o => o != AuditLogItemType.None)
            .Select(o => new { Key = o.ToString(), Value = data.TryGetValue(o, out var val) ? val : 0 })
            .OrderByDescending(o => o.Value).ThenBy(o => o.Key)
            .ToDictionary(o => o.Key, o => o.Value);
    }

    private static async Task ProcessFileStatisticsAsync(AuditLogStatistics statistics, GrillBotRepository repository)
    {
        var files = await repository.AuditLog.GetAllFilesAsync();
        var group = files.GroupBy(o => o.Extension)
            .Select(o => new { o.Key, Count = o.Count(), Size = o.Sum(x => x.Size) })
            .ToList();

        statistics.FileCounts = group.ToDictionary(o => o.Key, o => o.Count);
        statistics.FileSizes = group.ToDictionary(o => o.Key, o => o.Size);
    }
}
