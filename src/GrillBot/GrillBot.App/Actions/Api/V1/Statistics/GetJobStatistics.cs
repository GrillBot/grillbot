using GrillBot.App.Services.AuditLog;
using GrillBot.Common.Models;
using GrillBot.Data.Models.API.AuditLog.Filters;
using GrillBot.Data.Models.API.Statistics;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Actions.Api.V1.Statistics;

public class GetJobStatistics : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public GetJobStatistics(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task<List<StatisticItem>> ProcessAsync()
    {
        var filterModel = new AuditLogListParams
        {
            Types = new List<AuditLogItemType> { AuditLogItemType.JobCompleted },
            Sort = null
        };

        await using var repository = DatabaseBuilder.CreateRepository();
        var data = await repository.AuditLog.GetSimpleDataAsync(filterModel);

        return data
            .Select(o => new
            {
                o.CreatedAt,
                Data = JsonConvert.DeserializeObject<JobExecutionData>(o.Data, AuditLogWriter.SerializerSettings)
            })
            .GroupBy(o => o.Data!.JobName)
            .Select(o => new StatisticItem
            {
                Key = o.Key,
                FailedCount = o.Count(x => x.Data!.WasError),
                Last = o.Max(x => x.CreatedAt),
                MaxDuration = o.Max(x => Convert.ToInt32((x.Data!.EndAt - x.Data.StartAt).TotalMilliseconds)),
                MinDuration = o.Min(x => Convert.ToInt32((x.Data!.EndAt - x.Data.StartAt).TotalMilliseconds)),
                SuccessCount = o.Count(x => !x.Data!.WasError),
                TotalDuration = o.Sum(x => Convert.ToInt32((x.Data!.EndAt - x.Data.StartAt).TotalMilliseconds)),
                LastRunDuration = o.OrderByDescending(x => x.CreatedAt).Select(x => Convert.ToInt32((x.Data!.EndAt - x.Data.StartAt).TotalMilliseconds)).FirstOrDefault()
            })
            .OrderByDescending(o => o.AvgDuration).ThenByDescending(o => o.SuccessCount + o.FailedCount).ThenBy(o => o.Key)
            .ToList();
    }
}
