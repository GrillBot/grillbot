using GrillBot.App.Services.AuditLog;
using GrillBot.Common.Models;
using GrillBot.Data.Models.API.AuditLog.Filters;
using GrillBot.Data.Models.API.Statistics;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Actions.Api.V1.Statistics;

public class GetApiStatistics : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public GetApiStatistics(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task<Dictionary<string, int>> ProcessByDateAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        return await repository.AuditLog.GetApiRequestsByDateAsync();
    }

    public async Task<List<StatisticItem>> ProcessByEndpointAsync()
    {
        var logItems = await GetLogItemsAsync();

        return logItems
            .GroupBy(o => $"{o.request!.Method} {o.request.TemplatePath}")
            .Select(o => new StatisticItem
            {
                Key = o.Key,
                FailedCount = o.Count(x => Convert.ToInt32(x.request!.StatusCode.Split(' ')[0]) >= 400),
                Last = o.Max(x => x.createdAt),
                MaxDuration = o.Max(x => Convert.ToInt32((x.request!.EndAt - x.request.StartAt).TotalMilliseconds)),
                MinDuration = o.Min(x => Convert.ToInt32((x.request!.EndAt - x.request.StartAt).TotalMilliseconds)),
                SuccessCount = o.Count(x => Convert.ToInt32(x.request!.StatusCode.Split(' ')[0]) < 400),
                TotalDuration = o.Sum(x => Convert.ToInt32((x.request!.EndAt - x.request.StartAt).TotalMilliseconds)),
                LastRunDuration = o.OrderByDescending(x => x.createdAt).Select(x => Convert.ToInt32((x.request!.EndAt - x.request.StartAt).TotalMilliseconds)).FirstOrDefault()
            })
            .OrderByDescending(o => o.AvgDuration).ThenByDescending(o => o.SuccessCount + o.FailedCount).ThenBy(o => o.Key)
            .ToList();
    }

    public async Task<Dictionary<string, int>> ProcessByStatusCodeAsync()
    {
        var logItems = await GetLogItemsAsync();

        return logItems
            .Select(o => new
            {
                o.createdAt,
                StatusCode = o.request!.GetStatusCode()
            })
            .GroupBy(o => o.StatusCode)
            .Select(o => new { o.Key, Count = o.Count() })
            .OrderByDescending(o => o.Count).ThenBy(o => o.Key)
            .ToDictionary(o => o.Key, o => o.Count);
    }

    private async Task<List<(DateTime createdAt, ApiRequest request)>> GetLogItemsAsync()
    {
        var parameters = new AuditLogListParams
        {
            Types = new List<AuditLogItemType> { AuditLogItemType.Api },
            Sort = null
        };

        await using var repository = DatabaseBuilder.CreateRepository();
        var data = await repository.AuditLog.GetSimpleDataAsync(parameters);

        return data
            .Select(o => (o.CreatedAt, JsonConvert.DeserializeObject<ApiRequest>(o.Data, AuditLogWriter.SerializerSettings)!))
            .Where(o => !o.Item2!.IsCorrupted())
            .ToList();
    }
}
