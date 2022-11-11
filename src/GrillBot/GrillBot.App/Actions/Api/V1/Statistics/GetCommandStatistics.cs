using GrillBot.App.Services.AuditLog;
using GrillBot.Common.Models;
using GrillBot.Data.Models.API.AuditLog.Filters;
using GrillBot.Data.Models.API.Statistics;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Actions.Api.V1.Statistics;

public class GetCommandStatistics : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public GetCommandStatistics(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task<List<StatisticItem>> ProcessInteractionsAsync()
    {
        var logItems = await GetLogItemsAsync();

        return logItems
            .GroupBy(o => o.data.FullName)
            .Select(o => new StatisticItem
            {
                Key = o.Key,
                FailedCount = o.Count(x => !x.data.IsSuccess),
                Last = o.Max(x => x.createdAt),
                MaxDuration = o.Max(x => x.data.Duration),
                MinDuration = o.Min(x => x.data.Duration),
                SuccessCount = o.Count(x => x.data.IsSuccess),
                TotalDuration = o.Sum(x => x.data.Duration),
                LastRunDuration = o.MaxBy(x => x.createdAt).data.Duration
            })
            .OrderByDescending(o => o.AvgDuration).ThenByDescending(o => o.SuccessCount + o.FailedCount).ThenBy(o => o.Key)
            .ToList();
    }

    private async Task<List<(DateTime createdAt, InteractionCommandExecuted data)>> GetLogItemsAsync()
    {
        var parameters = new AuditLogListParams
        {
            Types = new List<AuditLogItemType> { AuditLogItemType.InteractionCommand },
            IgnoreBots = true,
            Sort = null
        };

        await using var repository = DatabaseBuilder.CreateRepository();
        var data = await repository.AuditLog.GetSimpleDataAsync(parameters);

        return data
            .ConvertAll(o => (o.CreatedAt, JsonConvert.DeserializeObject<InteractionCommandExecuted>(o.Data, AuditLogWriter.SerializerSettings)!));
    }
}
