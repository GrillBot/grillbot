using GrillBot.Cache.Services;
using GrillBot.Cache.Services.Repository;
using GrillBot.Database.Services;
using GrillBot.Tests.Infrastructure.Cache;
using System;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Tests.Common;

[ExcludeFromCodeCoverage]
public abstract class ServiceTest<TService> where TService : class
{
    protected TService Service { get; set; }
    protected GrillBotContext DbContext { get; set; }
    protected GrillBotDatabaseFactory DbFactory { get; set; }
    protected GrillBotCacheRepository CacheRepository { get; set; }
    protected GrillBotCacheBuilder CacheBuilder { get; set; }

    protected abstract TService CreateService();

    [TestInitialize]
    public void Initialize()
    {
        DbFactory = new DbContextBuilder();
        DbContext = DbFactory.Create();

        CacheBuilder = new TestCacheBuilder();
        CacheRepository = CacheBuilder.CreateRepository();

        Service = CreateService();
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

        if (Service is IDisposable disposable)
            disposable.Dispose();

        if (Service is IAsyncDisposable asyncDisposable)
            asyncDisposable.DisposeAsync().AsTask().Wait();
    }
}
