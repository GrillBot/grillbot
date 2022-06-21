using GrillBot.Cache.Services;
using GrillBot.Cache.Services.Repository;
using GrillBot.Database.Services;
using GrillBot.Tests.Infrastructure.Cache;
using Moq;
using Quartz;
using System;
using System.Diagnostics.CodeAnalysis;
using GrillBot.Database.Services.Repository;
using GrillBot.Tests.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Tests.Common;

[ExcludeFromCodeCoverage]
public abstract class JobTest<TJob> where TJob : IJob
{
    protected TJob Job { get; set; }
    protected TestDatabaseBuilder DatabaseBuilder { get; private set; }
    protected TestCacheBuilder CacheBuilder { get; private set; }

    protected GrillBotRepository Repository { get; private set; }
    protected GrillBotCacheRepository CacheRepository { get; private set; }

    protected abstract TJob CreateJob();

    [TestInitialize]
    public void Initialize()
    {
        DatabaseBuilder = new TestDatabaseBuilder();
        CacheBuilder = new TestCacheBuilder();

        Repository = DatabaseBuilder.CreateRepository();
        CacheRepository = CacheBuilder.CreateRepository();

        Job = CreateJob();
    }

    public virtual void Cleanup() { }

    [TestCleanup]
    public void TestClean()
    {
        Cleanup();

        TestDatabaseBuilder.ClearDatabase();
        TestCacheBuilder.ClearDatabase();

        Repository.Dispose();
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
        mock.SetupProperty(o => o.Result);

        return mock.Object;
    }
}
