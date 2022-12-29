using System.Diagnostics.CodeAnalysis;
using GrillBot.App.Actions.Api.V1.ScheduledJobs;
using GrillBot.Cache.Services.Managers;
using GrillBot.Data.Exceptions;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Quartz;

namespace GrillBot.Tests.App.Actions.Api.V1.ScheduledJobs;

[TestClass]
public class UpdateJobTests : ApiActionTest<UpdateJob>
{
    protected override UpdateJob CreateAction()
    {
        var dataCacheManager = new DataCacheManager(CacheBuilder);
        var scheduler = new SchedulerBuilder()
            .SetGetJobKeysAction(new[] { Quartz.JobKey.Create("Job") })
            .Build();
        var schedulerFactory = new SchedulerFactoryBuilder().SetGetSchedulerAction(scheduler).Build();
        return new UpdateJob(ApiRequestContext, dataCacheManager, schedulerFactory, TestServices.Texts.Value);
    }

    [TestMethod]
    [ExcludeFromCodeCoverage]
    [ExpectedException(typeof(NotFoundException))]
    public async Task ProcessAsync_JobNotFound() => await Action.ProcessAsync("Job2", true);

    [TestMethod]
    public async Task ProcessAsync()
    {
        await Action.ProcessAsync("Job", true);
        await Action.ProcessAsync("Job", false);
    }
}
