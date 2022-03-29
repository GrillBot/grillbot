using GrillBot.Database.Services;
using System;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Tests.Common;

[ExcludeFromCodeCoverage]
public abstract class ServiceTest<TService> where TService : class
{
    protected TService Service { get; set; }
    protected GrillBotContext DbContext { get; set; }
    protected GrillBotContextFactory DbFactory { get; set; }

    protected abstract TService CreateService();
    internal TService BuildService() => CreateService();

    [TestInitialize]
    public void Initialize()
    {
        DbFactory = new DbContextBuilder();
        DbContext = DbFactory.Create();

        Service = CreateService();
    }

    public virtual void Cleanup() { }

    [TestCleanup]
    public void TestClean()
    {
        DbContext.ChangeTracker.Clear();

        Cleanup();

        DbContext.Dispose();

        if (Service is IDisposable disposable)
            disposable.Dispose();

        if (Service is IAsyncDisposable asyncDisposable)
            asyncDisposable.DisposeAsync().AsTask().Wait();
    }
}
