using GrillBot.App.Actions.Api.V1.ScheduledJobs;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Quartz;

namespace GrillBot.Tests.App.Actions.Api.V1.ScheduledJobs;

[TestClass]
public class RunScheduledJobTests : ApiActionTest<RunScheduledJob>
{
    protected override RunScheduledJob CreateInstance()
    {
        var scheduler = new SchedulerBuilder().Build();
        var schedulerFactory = new SchedulerFactoryBuilder().SetGetSchedulerAction(scheduler).Build();
    
        return new RunScheduledJob(ApiRequestContext, schedulerFactory);
    }

    [TestMethod]
    public async Task ProcessAsync()
    {
        await Instance.ProcessAsync("Job");
    }
}
