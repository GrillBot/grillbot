using Moq;
using Quartz;
using Quartz.Impl.Matchers;

namespace GrillBot.Tests.Infrastructure.Quartz;

public class SchedulerBuilder : BuilderBase<IScheduler>
{
    public SchedulerBuilder()
    {
        Mock.Setup(o => o.TriggerJob(It.IsAny<JobKey>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
    }

    public SchedulerBuilder SetGetJobKeysAction(IReadOnlyCollection<JobKey> jobKeys)
    {
        Mock.Setup(o => o.GetJobKeys(It.IsAny<GroupMatcher<JobKey>>(), It.IsAny<CancellationToken>())).ReturnsAsync(jobKeys);
        return this;
    }

    public SchedulerBuilder SetGetCurrentlyExecutingJobsAction(IReadOnlyCollection<IJobExecutionContext> jobs)
    {
        Mock.Setup(o => o.GetCurrentlyExecutingJobs(It.IsAny<CancellationToken>())).ReturnsAsync(jobs);
        return this;
    }

    public SchedulerBuilder SetGetTriggerAction(ITrigger trigger)
    {
        Mock.Setup(o => o.GetTrigger(It.Is<TriggerKey>(x => x.Name == trigger.Key.Name), It.IsAny<CancellationToken>())).ReturnsAsync(trigger);
        return this;
    }
}
