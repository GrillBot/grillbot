using GrillBot.Cache.Services.Repository;
using System.Diagnostics.CodeAnalysis;
using GrillBot.Database.Services.Repository;

namespace GrillBot.Tests.Common;

[ExcludeFromCodeCoverage]
public abstract class ServiceTest<TService> where TService : class
{
    protected TService Service { get; private set; }

    protected TestDatabaseBuilder DatabaseBuilder { get; private set; }
    protected TestCacheBuilder CacheBuilder { get; private set; }

    protected GrillBotRepository Repository { get; private set; }
    protected GrillBotCacheRepository CacheRepository { get; private set; }

    protected abstract TService CreateService();

    [TestInitialize]
    public void Initialize()
    {
        DatabaseBuilder = TestServices.DatabaseBuilder.Value;
        CacheBuilder = TestServices.CacheBuilder.Value;
        Repository = DatabaseBuilder.CreateRepository();
        CacheRepository = CacheBuilder.CreateRepository();

        Service = CreateService();
    }

    [TestCleanup]
    public void TestClean()
    {
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
