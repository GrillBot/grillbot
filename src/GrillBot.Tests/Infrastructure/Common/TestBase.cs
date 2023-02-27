using System.Reflection;
using GrillBot.Cache.Services.Repository;
using GrillBot.Database.Services.Repository;

namespace GrillBot.Tests.Infrastructure.Common;

public abstract class TestBase<TTestClass> where TTestClass : class
{
    private GrillBotRepository? _botRepository;
    private GrillBotCacheRepository? _cacheRepository;
    private bool _canDisposeDatabase;
    private bool _canDisposeCache;
    private bool _usedDatabase;
    private bool _usedCache;

    protected TTestClass Instance { get; private set; } = null!;

    protected TestDatabaseBuilder DatabaseBuilder
    {
        get
        {
            _usedDatabase = true;
            return TestServices.DatabaseBuilder.Value;
        }
    }

    protected TestCacheBuilder CacheBuilder
    {
        get
        {
            _usedCache = true;
            return TestServices.CacheBuilder.Value;
        }
    }

    protected GrillBotRepository Repository
    {
        get
        {
            _botRepository ??= DatabaseBuilder.CreateRepository();
            _canDisposeDatabase = true;

            return _botRepository;
        }
    }

    protected GrillBotCacheRepository CacheRepository
    {
        get
        {
            _cacheRepository ??= CacheBuilder.CreateRepository();
            _canDisposeCache = true;

            return _cacheRepository;
        }
    }

    public TestContext TestContext { get; set; } = null!;

    protected MethodInfo GetMethod()
    {
        var type = GetType();
        return type.GetMethod(TestContext.ManagedMethod!, BindingFlags.Instance | BindingFlags.Public)!;
    }

    [TestInitialize]
    public void Initialization()
    {
        PreInit();
        Instance = CreateInstance();
        PostInit();
    }

    protected abstract TTestClass CreateInstance();

    protected virtual void PreInit()
    {
    }

    protected virtual void PostInit()
    {
    }

    protected virtual void Cleanup()
    {
    }

    [TestCleanup]
    public void Clean()
    {
        Cleanup();

        if (_canDisposeCache)
        {
            _cacheRepository!.Dispose();
            _cacheRepository = null;
        }

        if (_usedCache)
            TestCacheBuilder.ClearDatabase();

        if (_canDisposeDatabase)
        {
            _botRepository!.Dispose();
            _botRepository = null;
        }

        if (_usedDatabase)
            TestDatabaseBuilder.ClearDatabase();

        if (Instance is IDisposable disposable)
            disposable.Dispose();
        if (Instance is IAsyncDisposable asyncDisposable)
            asyncDisposable.DisposeAsync().AsTask().Wait();
    }
}
