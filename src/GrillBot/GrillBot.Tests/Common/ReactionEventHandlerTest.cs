using GrillBot.App.Infrastructure;
using GrillBot.Cache.Services;
using GrillBot.Cache.Services.Repository;
using GrillBot.Database.Services;
using GrillBot.Tests.Infrastructure.Cache;
using System;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Tests.Common;

[ExcludeFromCodeCoverage]
public abstract class ReactionEventHandlerTest<THandler> where THandler : ReactionEventHandler
{
    protected THandler Handler { get; set; }
    protected GrillBotContext DbContext { get; set; }
    protected GrillBotDatabaseBuilder DbFactory { get; set; }
    protected GrillBotCacheRepository CacheRepository { get; set; }
    protected GrillBotCacheBuilder CacheBuilder { get; set; }

    protected abstract THandler CreateHandler();

    [TestInitialize]
    public void Initialize()
    {
        DbFactory = new DbContextBuilder();
        DbContext = DbFactory.Create();

        CacheBuilder = new TestCacheBuilder();
        CacheRepository = CacheBuilder.CreateRepository();

        Handler = CreateHandler();
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

        if (Handler is IDisposable disposable)
            disposable.Dispose();

        if (Handler is IAsyncDisposable asyncDisposable)
            asyncDisposable.DisposeAsync().AsTask().Wait();
    }
}
