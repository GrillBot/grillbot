using Quartz;

namespace GrillBot.Tests.Infrastructure.Quartz;

public class JobExecutionContextBuilder : BuilderBase<IJobExecutionContext>
{
    public JobExecutionContextBuilder SetJobDetail(IJobDetail jobDetail)
    {
        Mock.Setup(o => o.JobDetail).Returns(jobDetail);
        return this;
    }
}
