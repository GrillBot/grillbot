using GrillBot.App.Actions.Api.V1.ScheduledJobs;
using GrillBot.App.Jobs;
using GrillBot.Cache.Services.Managers;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Quartz;
using Newtonsoft.Json;
using Quartz.Impl;

namespace GrillBot.Tests.App.Actions.Api.V1.ScheduledJobs;

[TestClass]
public class GetScheduledJobsTests : ApiActionTest<GetScheduledJobs>
{
    protected override GetScheduledJobs CreateInstance()
    {
        var jobExecution = new JobExecutionContextBuilder().SetJobDetail(new JobDetailImpl("Job2", typeof(MessageCacheJob))).Build();
        var scheduler = new SchedulerBuilder()
            .SetGetJobKeysAction(new[] { Quartz.JobKey.Create("Job"), Quartz.JobKey.Create("Job2") })
            .SetGetCurrentlyExecutingJobsAction(new[] { jobExecution })
            .SetGetTriggerAction(new TriggerBuilder().SetKey(new Quartz.TriggerKey("Job-Trigger")).SetGetNextFireTimeUtc(DateTimeOffset.Now).Build())
            .SetGetTriggerAction(new TriggerBuilder().SetKey(new Quartz.TriggerKey("Job2-Trigger")).SetGetNextFireTimeUtc(DateTimeOffset.Now).Build())
            .Build();

        var schedulerFactory = new SchedulerFactoryBuilder().SetGetSchedulerAction(scheduler).Build();
        var dataCacheManager = new DataCacheManager(CacheBuilder);

        return new GetScheduledJobs(ApiRequestContext, DatabaseBuilder, schedulerFactory, dataCacheManager);
    }

    private async Task InitDataAsync()
    {
        await Repository.AddCollectionAsync(new[]
        {
            new AuditLogItem
            {
                Data = JsonConvert.SerializeObject(new JobExecutionData
                {
                    Result = "Error",
                    EndAt = DateTime.Now.AddDays(1),
                    StartAt = DateTime.Now.AddDays(-1),
                    JobName = "Job",
                    WasError = true
                }),
                Type = AuditLogItemType.JobCompleted,
                CreatedAt = DateTime.Now.AddHours(1)
            },
            new AuditLogItem
            {
                Data = JsonConvert.SerializeObject(new JobExecutionData
                {
                    Result = "Success",
                    EndAt = DateTime.Now.AddDays(1.5),
                    StartAt = DateTime.Now.AddDays(-0.5),
                    JobName = "Job",
                    WasError = false
                }),
                Type = AuditLogItemType.JobCompleted,
                CreatedAt = DateTime.Now.AddHours(1)
            }
        });
        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync()
    {
        await InitDataAsync();

        var result = await Instance.ProcessAsync();
        Assert.AreEqual(2, result.Count);
    }
}
