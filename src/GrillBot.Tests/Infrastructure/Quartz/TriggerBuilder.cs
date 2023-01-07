using Quartz;

namespace GrillBot.Tests.Infrastructure.Quartz;

public class TriggerBuilder : BuilderBase<ITrigger>
{
    public TriggerBuilder SetKey(TriggerKey key)
    {
        Mock.Setup(o => o.Key).Returns(key);
        return this;
    }

    public TriggerBuilder SetGetNextFireTimeUtc(DateTimeOffset nextFireTime)
    {
        Mock.Setup(o => o.GetNextFireTimeUtc()).Returns(nextFireTime);
        return this;
    }
}
