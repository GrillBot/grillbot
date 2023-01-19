using System.Diagnostics.CodeAnalysis;
using GrillBot.Database.Services.Repository;

namespace GrillBot.Tests.Infrastructure.Common;

[ExcludeFromCodeCoverage]
public abstract class HandlerTest<THandler>
{
    protected THandler Handler { get; private set; } = default!;

    protected TestDatabaseBuilder DatabaseBuilder { get; private set; } = null!;
    protected GrillBotRepository Repository { get; private set; } = null!;

    protected abstract THandler CreateHandler();

    [TestInitialize]
    public void TestInitialization()
    {
        DatabaseBuilder = TestServices.DatabaseBuilder.Value;
        Repository = DatabaseBuilder.CreateRepository();
        
        Handler = CreateHandler();
    }

    [TestCleanup]
    public void TestClean()
    {
        TestDatabaseBuilder.ClearDatabase();
        Repository.Dispose();
        
        if (Handler is IDisposable disposable)
            disposable.Dispose();
    }
}
