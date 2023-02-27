using Moq;
using Quartz;

namespace GrillBot.Tests.Infrastructure.Common;

public abstract class JobTest<TJob> : TestBase<TJob> where TJob : class, IJob
{
    protected async Task Execute(Action<IJobExecutionContext>? checks = null)
    {
        var context = CreateContext();
        await Instance.Execute(context);

        checks?.Invoke(context);
    }

    protected IJobExecutionContext CreateContext()
    {
        var mock = new Mock<IJobExecutionContext>();
        mock.Setup(o => o.CancellationToken).Returns(CancellationToken.None);
        mock.SetupProperty(o => o.Result);

        return mock.Object;
    }
}
