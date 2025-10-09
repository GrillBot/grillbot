using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using AuditLog;
using AuditLog.Models.Response.Info;
using GrillBot.Core.Services.Common.Executor;
using GrillBot.Data.Models.API.Jobs;
using Quartz;
using Quartz.Impl.Matchers;

namespace GrillBot.App.Actions.Api.V1.ScheduledJobs;

public class GetScheduledJobs : ApiAction
{
    private ISchedulerFactory SchedulerFactory { get; }
    private readonly DataCacheManager _dataCacheManager;
    private readonly IServiceClientExecutor<IAuditLogServiceClient> _auditLogServiceClient;

    public GetScheduledJobs(ApiRequestContext apiContext, ISchedulerFactory schedulerFactory, DataCacheManager dataCacheManager,
        IServiceClientExecutor<IAuditLogServiceClient> auditLogServiceClient) : base(apiContext)
    {
        SchedulerFactory = schedulerFactory;
        _dataCacheManager = dataCacheManager;
        _auditLogServiceClient = auditLogServiceClient;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var jobInfos = await _auditLogServiceClient.ExecuteRequestAsync((c, ctx) => c.GetJobsInfoAsync(ctx.CancellationToken));
        var scheduler = await SchedulerFactory.GetScheduler();
        var result = new List<ScheduledJob>();
        var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup());
        var runningJobs = await scheduler.GetCurrentlyExecutingJobs();
        var disabledJobs = await GetDisabledJobsAsync();

        foreach (var jobKey in jobKeys.OrderBy(o => o.Name))
        {
            var jobInfo = jobInfos.Find(o => o.Name == jobKey.Name) ?? new JobInfo { Name = jobKey.Name };
            result.Add(await GetJobAsync(jobKey, jobInfo, runningJobs, scheduler, disabledJobs));
        }

        return ApiResult.Ok(result);
    }

    private static async Task<ScheduledJob> GetJobAsync(JobKey key, JobInfo jobInfo, IEnumerable<IJobExecutionContext> runningJobs, IScheduler scheduler, ICollection<string> disabledJobs)
    {
        var trigger = await scheduler.GetTrigger(new TriggerKey($"{key.Name}-Trigger"));

        return new ScheduledJob
        {
            Name = key.Name,
            StartCount = jobInfo.StartCount,
            Running = runningJobs.Any(o => o.JobDetail.Key.Name == key.Name),
            NextRun = trigger!.GetNextFireTimeUtc()!.Value.LocalDateTime,
            IsActive = !disabledJobs.Contains(key.Name),
            LastRunDuration = jobInfo.LastRunDuration,
            LastRun = jobInfo.LastStartAt,
            MaxTime = jobInfo.MaxTime,
            MinTime = jobInfo.MinTime,
            AverageTime = jobInfo.AvgTime,
            FailedCount = jobInfo.FailedCount,
            TotalTime = jobInfo.TotalDuration
        };
    }

    private async Task<List<string>> GetDisabledJobsAsync()
    {
        var data = await _dataCacheManager.GetValueAsync<List<string>>("DisabledJobs");
        return data ?? new();
    }
}
