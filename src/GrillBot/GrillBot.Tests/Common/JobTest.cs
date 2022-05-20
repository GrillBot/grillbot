using GrillBot.Cache.Services;
using GrillBot.Cache.Services.Repository;
using GrillBot.Database.Services;
using GrillBot.Tests.Infrastructure.Cache;
using Moq;
using Quartz;
using System;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Tests.Common;

[ExcludeFromCodeCoverage]
public abstract class JobTest<TJob> where TJob : IJob
{
    protected TJob Job { get; set; }
    protected GrillBotContext DbContext { get; set; }
    protected GrillBotContextFactory DbFactory { get; set; }
    protected GrillBotCacheBuilder CacheBuilder { get; set; }
    protected GrillBotCacheRepository CacheRepository { get; set; }

    protected abstract TJob CreateJob();

    [TestInitialize]
    public void Initialize()
    {
        DbFactory = new DbContextBuilder();
        DbContext = DbFactory.Create();

        CacheBuilder = new TestCacheBuilder();
        CacheRepository = CacheBuilder.CreateRepository();

        Job = CreateJob();
    }

    public virtual void Cleanup() { }

    [TestCleanup]
    public void TestClean()
    {
        DbContext.ChangeTracker.Clear();
        DatabaseHelper.ClearDatabase(DbContext);
        TestCacheBuilder.ClearDatabase(CacheRepository);

        Cleanup();

        DbContext.Dispose();
        CacheRepository.Dispose();

        if (Job is IDisposable disposable)
            disposable.Dispose();

        if (Job is IAsyncDisposable asyncDisposable)
            asyncDisposable.DisposeAsync().AsTask().Wait();
    }

    protected IJobExecutionContext CreateContext()
    {
        var mock = new Mock<IJobExecutionContext>();
        mock.Setup(o => o.CancellationToken).Returns(CancellationToken.None);

        return mock.Object;
    }
}
