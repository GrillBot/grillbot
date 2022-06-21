using GrillBot.App.Infrastructure;
using GrillBot.Cache.Services;
using GrillBot.Cache.Services.Repository;
using GrillBot.Database.Services;
using GrillBot.Tests.Infrastructure.Cache;
using System;
using System.Diagnostics.CodeAnalysis;
using GrillBot.Database.Services.Repository;
using GrillBot.Tests.Infrastructure.Database;

namespace GrillBot.Tests.Common;

[ExcludeFromCodeCoverage]
public abstract class ReactionEventHandlerTest<THandler> where THandler : ReactionEventHandler
{
    protected THandler Handler { get; set; }

    protected TestDatabaseBuilder DatabaseBuilder { get; private set; }
    protected TestCacheBuilder CacheBuilder { get; private set; }

    protected GrillBotRepository Repository { get; private set; }
    protected GrillBotCacheRepository CacheRepository { get; private set; }

    protected abstract THandler CreateHandler();

    [TestInitialize]
    public void Initialize()
    {
        DatabaseBuilder = new TestDatabaseBuilder();
        CacheBuilder = new TestCacheBuilder();

        Repository = DatabaseBuilder.CreateRepository();
        CacheRepository = CacheBuilder.CreateRepository();

        Handler = CreateHandler();
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

        if (Handler is IDisposable disposable)
            disposable.Dispose();

        if (Handler is IAsyncDisposable asyncDisposable)
            asyncDisposable.DisposeAsync().AsTask().Wait();
    }
}
