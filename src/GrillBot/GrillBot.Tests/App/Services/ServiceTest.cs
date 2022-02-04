using GrillBot.Database.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Tests.App.Services;

[ExcludeFromCodeCoverage]
public abstract class ServiceTest<TService> where TService : class
{
    protected TService Service { get; set; }
    protected GrillBotContext DbContext { get; set; }

    protected abstract TService CreateService();
    internal TService BuildService() => CreateService();

    [TestInitialize]
    public void Initialize()
    {
        Service = CreateService();
    }

    public virtual void Cleanup() { }

    [TestCleanup]
    public void TestClean()
    {
        Cleanup();

        DbContext?.Dispose();
        if (Service is IDisposable disposable)
            disposable.Dispose();
    }
}
