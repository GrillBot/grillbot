using Moq;
using Quartz;

namespace GrillBot.Tests.Infrastructure.Quartz;

public class SchedulerFactoryBuilder : BuilderBase<ISchedulerFactory>
{
    public SchedulerFactoryBuilder SetGetSchedulerAction(IScheduler scheduler)
    {
        Mock.Setup(o => o.GetScheduler(It.IsAny<CancellationToken>())).ReturnsAsync(scheduler);
        return this;
    }
}
