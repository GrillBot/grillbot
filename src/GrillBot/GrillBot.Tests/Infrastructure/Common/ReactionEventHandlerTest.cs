using GrillBot.App.Infrastructure;
using GrillBot.Cache.Services.Repository;
using System.Diagnostics.CodeAnalysis;
using GrillBot.Database.Services.Repository;

namespace GrillBot.Tests.Common;

[ExcludeFromCodeCoverage]
public abstract class ReactionEventHandlerTest<THandler> where THandler : ReactionEventHandler
{
    protected THandler Handler { get; private set; }

    protected TestDatabaseBuilder DatabaseBuilder { get; private set; }
    protected TestCacheBuilder CacheBuilder { get; private set; }

    protected GrillBotRepository Repository { get; private set; }
    protected GrillBotCacheRepository CacheRepository { get; private set; }

    protected abstract THandler CreateHandler();

    [TestInitialize]
    public void Initialize()
    {
        DatabaseBuilder = TestServices.DatabaseBuilder.Value;
        CacheBuilder = TestServices.CacheBuilder.Value;
        Repository = DatabaseBuilder.CreateRepository();
        CacheRepository = CacheBuilder.CreateRepository();

        Handler = CreateHandler();
    }

    [TestCleanup]
    public void TestClean()
    {
        TestDatabaseBuilder.ClearDatabase();
        TestCacheBuilder.ClearDatabase();

        Repository.Dispose();
        CacheRepository.Dispose();
    }
}
