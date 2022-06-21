using GrillBot.Cache.Services.Repository;
using GrillBot.Tests.Infrastructure.Cache;
using System;
using System.Diagnostics.CodeAnalysis;
using GrillBot.Database.Services.Repository;
using GrillBot.Tests.Infrastructure.Database;

namespace GrillBot.Tests.Common;

[ExcludeFromCodeCoverage]
public abstract class ServiceTest<TService> where TService : class
{
    protected TService Service { get; set; }

    protected TestDatabaseBuilder DatabaseBuilder { get; private set; }
    protected TestCacheBuilder CacheBuilder { get; private set; }

    protected GrillBotRepository Repository { get; private set; }
    protected GrillBotCacheRepository CacheRepository { get; private set; }

    protected abstract TService CreateService();

    [TestInitialize]
    public void Initialize()
    {
        DatabaseBuilder = new TestDatabaseBuilder();
        CacheBuilder = new TestCacheBuilder();

        Repository = DatabaseBuilder.CreateRepository();
        CacheRepository = CacheBuilder.CreateRepository();

        Service = CreateService();
    }

    public virtual void Cleanup()
    {
    }

    [TestCleanup]
    public void TestClean()
    {
        Cleanup();

        TestDatabaseBuilder.ClearDatabase();
        TestCacheBuilder.ClearDatabase();

        Repository.Dispose();
        CacheRepository.Dispose();

        if (Service is IDisposable disposable)
            disposable.Dispose();

        if (Service is IAsyncDisposable asyncDisposable)
            asyncDisposable.DisposeAsync().AsTask().Wait();
    }
}
