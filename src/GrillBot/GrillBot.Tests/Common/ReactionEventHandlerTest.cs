using GrillBot.App.Infrastructure;
using GrillBot.Database.Services;
using System;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Tests.Common;

[ExcludeFromCodeCoverage]
public abstract class ReactionEventHandlerTest<THandler> where THandler : ReactionEventHandler
{
    protected THandler Handler { get; set; }
    protected GrillBotContext DbContext { get; set; }
    protected GrillBotContextFactory DbFactory { get; set; }

    protected abstract THandler CreateHandler();

    [TestInitialize]
    public void Initialize()
    {
        DbFactory = new DbContextBuilder();
        DbContext = DbFactory.Create();

        Handler = CreateHandler();
    }

    public virtual void Cleanup() { }

    [TestCleanup]
    public void TestClean()
    {
        DbContext.ChangeTracker.Clear();

        Cleanup();

        DbContext.Dispose();

        if (Handler is IDisposable disposable)
            disposable.Dispose();

        if (Handler is IAsyncDisposable asyncDisposable)
            asyncDisposable.DisposeAsync().AsTask().Wait();
    }
}
