using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using Quartz;

namespace GrillBot.App.Actions.Api.V1.ScheduledJobs;

public class RunScheduledJob : ApiAction
{
    private ISchedulerFactory SchedulerFactory { get; }

    public RunScheduledJob(ApiRequestContext apiContext, ISchedulerFactory schedulerFactory) : base(apiContext)
    {
        SchedulerFactory = schedulerFactory;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var name = (string)Parameters[0]!;
        var scheduler = await SchedulerFactory.GetScheduler();

        var jobData = new JobDataMap();
        jobData.Put("User", ApiContext.LoggedUser!);

        await scheduler.TriggerJob(JobKey.Create(name), jobData);
        return ApiResult.Ok();
    }
}
