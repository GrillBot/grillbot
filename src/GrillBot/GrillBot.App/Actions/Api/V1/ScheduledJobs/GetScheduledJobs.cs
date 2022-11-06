using GrillBot.App.Services.AuditLog;
using GrillBot.Common.Models;
using GrillBot.Data.Models.API.AuditLog.Filters;
using GrillBot.Data.Models.API.Jobs;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using Quartz;
using Quartz.Impl.Matchers;

namespace GrillBot.App.Actions.Api.V1.ScheduledJobs;

public class GetScheduledJobs : ApiAction
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private ISchedulerFactory SchedulerFactory { get; }

    public GetScheduledJobs(ApiRequestContext apiContext, GrillBotDatabaseBuilder databaseBuilder, ISchedulerFactory schedulerFactory) : base(apiContext)
    {
        DatabaseBuilder = databaseBuilder;
        SchedulerFactory = schedulerFactory;
    }

    public async Task<List<ScheduledJob>> ProcessAsync()
    {
        var logItems = await GetLogItemsAsync();
        var scheduler = await SchedulerFactory.GetScheduler();
        var result = new List<ScheduledJob>();
        var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
        var runningJobs = await scheduler.GetCurrentlyExecutingJobs();

        foreach (var jobKey in jobKeys.OrderBy(o => o.Name))
        {
            var logs = logItems.TryGetValue(jobKey.Name, out var logsData) ? logsData : new List<JobExecutionData>();
            var job = await GetJobAsync(jobKey, logs, runningJobs, scheduler);

            result.Add(job);
        }

        return result;
    }

    private static async Task<ScheduledJob> GetJobAsync(JobKey key, IReadOnlyList<JobExecutionData> logs, IEnumerable<IJobExecutionContext> runningJobs, IScheduler scheduler)
    {
        var trigger = await scheduler.GetTrigger(new TriggerKey($"{key.Name}-Trigger"));
        var newestItem = logs.Count == 0 ? null : logs[0];

        var job = new ScheduledJob
        {
            Name = key.Name,
            StartCount = logs.Count,
            Running = runningJobs.Any(o => o.JobDetail.Key.Name == key.Name),
            NextRun = trigger!.GetNextFireTimeUtc()!.Value.LocalDateTime,
            IsActive = true,
            LastRunDuration = newestItem?.Duration(),
            LastRun = newestItem?.StartAt,
            MaxTime = int.MinValue,
            MinTime = int.MaxValue
        };

        foreach (var logItem in logs)
        {
            var duration = logItem.Duration();
            if (logItem.WasError) job.FailedCount++;

            job.TotalTime += duration;
            if (duration > job.MaxTime) job.MaxTime = duration;
            if (duration < job.MinTime) job.MinTime = duration;
        }

        job.AverageTime = (int)Math.Ceiling(job.TotalTime / (double)job.StartCount);
        return job;
    }

    private async Task<Dictionary<string, List<JobExecutionData>>> GetLogItemsAsync()
    {
        var parameters = new AuditLogListParams
        {
            Sort = { Descending = true },
            Types = new List<AuditLogItemType> { AuditLogItemType.JobCompleted },
        };

        await using var repository = DatabaseBuilder.CreateRepository();
        var logItems = await repository.AuditLog.GetSimpleDataAsync(parameters);

        return logItems
            .Select(o => JsonConvert.DeserializeObject<JobExecutionData>(o.Data, AuditLogWriter.SerializerSettings)!)
            .GroupBy(o => o.JobName)
            .ToDictionary(o => o.Key, o => o.ToList());
    }
}
