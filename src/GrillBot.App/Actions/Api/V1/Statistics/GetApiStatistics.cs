using GrillBot.App.Managers;
using GrillBot.Common.Models;
using GrillBot.Data.Models.API.AuditLog.Filters;
using GrillBot.Data.Models.API.Statistics;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using GrillBot.Database.Models;
using GrillBot.Database.Services.Repository;

namespace GrillBot.App.Actions.Api.V1.Statistics;

public class GetApiStatistics : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public GetApiStatistics(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task<ApiStatistics> ProcessAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var requests = await GetLogItemsAsync(repository);
        var result = new ApiStatistics
        {
            Endpoints = BuildEndpointStatistics(requests)
        };

        foreach (var request in requests)
        {
            ProcessByStatusCode(result, request);
            ProcessByDate(result, request);
        }

        result.ByStatusCodeInternalApi = result.ByStatusCodeInternalApi.OrderByDescending(o => o.Value).ThenBy(o => o.Key).ToDictionary(o => o.Key, o => o.Value);
        result.ByStatusCodePublicApi = result.ByStatusCodePublicApi.OrderByDescending(o => o.Value).ThenBy(o => o.Key).ToDictionary(o => o.Key, o => o.Value);

        return result;
    }

    private static List<StatisticItem> BuildEndpointStatistics(IEnumerable<ApiRequest> requests)
    {
        return requests
            .GroupBy(o => $"{o.Method} {o.TemplatePath}")
            .Select(o => new StatisticItem
            {
                Key = o.Key,
                FailedCount = o.Count(x => Convert.ToInt32(x.StatusCode.Split(' ')[0]) >= 400),
                Last = o.Max(x => x.EndAt),
                MaxDuration = o.Max(x => x.Duration()),
                MinDuration = o.Min(x => x.Duration()),
                SuccessCount = o.Count(x => Convert.ToInt32(x.GetStatusCode()!.Split(' ')[0]) < 400),
                TotalDuration = o.Sum(x => x.Duration()),
                LastRunDuration = o.OrderByDescending(x => x.EndAt).Select(x => x.Duration()).FirstOrDefault()
            })
            .OrderByDescending(o => o.AvgDuration).ThenByDescending(o => o.SuccessCount + o.FailedCount).ThenBy(o => o.Key)
            .ToList();
    }

    private static void ProcessByStatusCode(ApiStatistics result, ApiRequest request)
    {
        var destination = request.ApiGroupName == "V2" ? result.ByStatusCodePublicApi : result.ByStatusCodeInternalApi;
        var statusCode = request.GetStatusCode()!;

        if (!destination.ContainsKey(statusCode))
            destination.Add(statusCode, 1);
        else
            destination[statusCode]++;
    }

    private static void ProcessByDate(ApiStatistics result, ApiRequest request)
    {
        var destination = request.ApiGroupName == "V2" ? result.ByDatePublicApi : result.ByDateInternalApi;
        var dateGroup = request.EndAt.ToString("MM-yyyy");

        if (!destination.ContainsKey(dateGroup))
            destination.Add(dateGroup, 1);
        else
            destination[dateGroup]++;
    }

    private static async Task<List<ApiRequest>> GetLogItemsAsync(GrillBotRepository repository)
    {
        var parameters = new AuditLogListParams
        {
            Types = new List<AuditLogItemType> { AuditLogItemType.Api },
            Sort = new SortParams { OrderBy = "CreatedAt", Descending = false }
        };

        var data = await repository.AuditLog.GetOnlyDataAsync(parameters);
        return data
            .Select(o => JsonConvert.DeserializeObject<ApiRequest>(o, AuditLogWriteManager.SerializerSettings)!)
            .Where(o => !o.IsCorrupted())
            .ToList();
    }
}
