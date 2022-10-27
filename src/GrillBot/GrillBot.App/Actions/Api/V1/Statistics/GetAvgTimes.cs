using GrillBot.App.Services.AuditLog;
using GrillBot.Common.Extensions;
using GrillBot.Common.Helpers;
using GrillBot.Common.Models;
using GrillBot.Data.Models.API.AuditLog.Filters;
using GrillBot.Data.Models.API.Statistics;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Actions.Api.V1.Statistics;

public class GetAvgTimes : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public GetAvgTimes(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task<AvgExecutionTimes> ProcessAsync()
    {
        var result = new AvgExecutionTimes();
        var logItems = await GetLogItemsAsync();

        if (logItems.TryGetValue(AuditLogItemType.InteractionCommand, out var interactionData))
        {
            result.Interactions = interactionData.GroupBy(o => o.createdAt.Date).OrderBy(o => o.Key)
                .ToDictionary(o => o.Key.ToCzechFormat(true), o => Math.Round(o.Select(x => (InteractionCommandExecuted)x.data).Average(x => x.Duration)));
        }

        if (logItems.TryGetValue(AuditLogItemType.JobCompleted, out var jobs))
        {
            result.Jobs = jobs.GroupBy(o => o.createdAt.Date).OrderBy(o => o.Key)
                .ToDictionary(o => o.Key.ToCzechFormat(true), o => Math.Round(o.Select(x => (JobExecutionData)x.data).Average(x => (x.EndAt - x.StartAt).TotalMilliseconds)));
        }

        if (!logItems.TryGetValue(AuditLogItemType.Api, out var api))
            return result;

        result.InternalApi = api.Select(o => (o.createdAt, (ApiRequest)o.data)).Where(o => (o.Item2.ApiGroupName ?? "V1") == "V1").GroupBy(o => o.createdAt.Date)
            .OrderBy(o => o.Key).ToDictionary(o => o.Key.ToCzechFormat(true), o => Math.Round(o.Average(x => (x.Item2.EndAt - x.Item2.StartAt).TotalMilliseconds)));

        result.ExternalApi = api.Select(o => (o.createdAt, (ApiRequest)o.data)).Where(o => (o.Item2.ApiGroupName ?? "V1") == "V2").GroupBy(o => o.createdAt.Date)
            .OrderBy(o => o.Key).ToDictionary(o => o.Key.ToCzechFormat(true), o => Math.Round(o.Average(x => (x.Item2.EndAt - x.Item2.StartAt).TotalMilliseconds)));

        return result;
    }

    private async Task<Dictionary<AuditLogItemType, List<(DateTime createdAt, object data)>>> GetLogItemsAsync()
    {
        var parameters = new AuditLogListParams
        {
            Types = new List<AuditLogItemType> { AuditLogItemType.Api, AuditLogItemType.InteractionCommand, AuditLogItemType.JobCompleted },
            Sort = null
        };

        await using var repository = DatabaseBuilder.CreateRepository();
        var data = await repository.AuditLog.GetSimpleDataAsync(parameters);

        var result = new Dictionary<AuditLogItemType, List<(DateTime createdAt, object data)>>();
        foreach (var item in data)
        {
            object deserializedData = null;
            switch (item.Type)
            {
                case AuditLogItemType.Api:
                {
                    var jsonData = JsonConvert.DeserializeObject<ApiRequest>(item.Data, AuditLogWriter.SerializerSettings)!;
                    if (!jsonData.IsCorrupted()) deserializedData = jsonData;
                    break;
                }
                case AuditLogItemType.InteractionCommand:
                    deserializedData = JsonConvert.DeserializeObject<InteractionCommandExecuted>(item.Data, AuditLogWriter.SerializerSettings);
                    break;
                case AuditLogItemType.JobCompleted:
                    deserializedData = JsonConvert.DeserializeObject<JobExecutionData>(item.Data, AuditLogWriter.SerializerSettings);
                    break;
            }

            if (deserializedData == null) continue;
            if (!result.ContainsKey(item.Type))
                result.Add(item.Type, new List<(DateTime createdAt, object data)>());

            result[item.Type].Add((item.CreatedAt, deserializedData));
        }

        return result;
    }
}
