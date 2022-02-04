using GrillBot.Database.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Quartz;
using System.Threading;

namespace GrillBot.Tests.App.Services;

public abstract class JobTest<TJob> where TJob : IJob
{
    protected TJob Job { get; set; }
    protected GrillBotContext DbContext { get; set; }

    protected abstract TJob CreateJob();

    [TestInitialize]
    public void Initialize()
    {
        Job = CreateJob();
    }

    public virtual void Cleanup() { }

    [TestCleanup]
    public void TestClean()
    {
        Cleanup();
        DbContext?.Dispose();
    }

    protected IJobExecutionContext CreateContext()
    {
        var mock = new Mock<IJobExecutionContext>();
        mock.Setup(o => o.CancellationToken).Returns(CancellationToken.None);

        return mock.Object;
    }
}
