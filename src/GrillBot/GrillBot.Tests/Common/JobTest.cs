using GrillBot.Cache.Services.Repository;
using Moq;
using Quartz;
using System.Diagnostics.CodeAnalysis;
using GrillBot.Database.Services.Repository;

namespace GrillBot.Tests.Common;

[ExcludeFromCodeCoverage]
public abstract class JobTest<TJob> where TJob : IJob
{
    protected TJob Job { get; private set; }
    protected TestDatabaseBuilder DatabaseBuilder { get; private set; }
    protected TestCacheBuilder CacheBuilder { get; private set; }

    protected GrillBotRepository Repository { get; private set; }
    protected GrillBotCacheRepository CacheRepository { get; private set; }

    protected abstract TJob CreateJob();

    [TestInitialize]
    public void Initialize()
    {
        DatabaseBuilder = TestServices.DatabaseBuilder.Value;
        CacheBuilder = TestServices.CacheBuilder.Value;
        Repository = DatabaseBuilder.CreateRepository();
        CacheRepository = CacheBuilder.CreateRepository();

        Job = CreateJob();
    }

    [TestCleanup]
    public void TestClean()
    {
        TestDatabaseBuilder.ClearDatabase();
        TestCacheBuilder.ClearDatabase();

        Repository.Dispose();
        CacheRepository.Dispose();

        if (Job is IDisposable disposable)
            disposable.Dispose();
    }

    protected IJobExecutionContext CreateContext()
    {
        var mock = new Mock<IJobExecutionContext>();
        mock.Setup(o => o.CancellationToken).Returns(CancellationToken.None);
        mock.SetupProperty(o => o.Result);

        return mock.Object;
    }
}
