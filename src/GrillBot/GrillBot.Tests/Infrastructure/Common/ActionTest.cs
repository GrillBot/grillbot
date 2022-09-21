using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using GrillBot.Cache.Services.Repository;
using GrillBot.Database.Services.Repository;

namespace GrillBot.Tests.Infrastructure.Common;

[ExcludeFromCodeCoverage]
public abstract class ActionTest : TestBase
{
    private ControllerTestConfiguration TestConfiguration
        => GetMethod().GetCustomAttribute<ControllerTestConfiguration>();

    protected bool IsPublic() => TestConfiguration?.IsPublic ?? false;
    //protected bool CanInitProvider() => TestConfiguration?.CanInitProvider ?? false;
}

[ExcludeFromCodeCoverage]
public abstract class ActionTest<TAction> : ActionTest
{
    protected TAction Action { get; private set; }

    protected TestDatabaseBuilder DatabaseBuilder { get; private set; }
    protected TestCacheBuilder CacheBuilder { get; private set; }
    protected GrillBotRepository Repository { get; private set; }
    protected GrillBotCacheRepository CacheRepository { get; private set; }

    protected abstract TAction CreateAction();

    [TestInitialize]
    public void TestInitialization()
    {
        DatabaseBuilder = TestServices.DatabaseBuilder.Value;
        CacheBuilder = TestServices.CacheBuilder.Value;
        Repository = DatabaseBuilder.CreateRepository();
        CacheRepository = CacheBuilder.CreateRepository();

        Init();
        Action = CreateAction();
    }

    protected abstract void Init();

    protected virtual void Cleanup()
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

        if (Action is IDisposable disposable)
            disposable.Dispose();
    }
}
