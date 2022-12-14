using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GrillBot.Database.Services;
using GrillBot.Database.Services.Repository;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Tests.Infrastructure.Database;

[ExcludeFromCodeCoverage]
public class TestDatabaseBuilder : GrillBotDatabaseBuilder
{
    private GrillBotContext Context { get; set; }

    public TestDatabaseBuilder() : base(null!, null!)
    {
    }

    private static DatabaseContext CreateContext()
    {
        var builder = new DbContextOptionsBuilder()
            .EnableDetailedErrors()
            .EnableThreadSafetyChecks()
            .EnableSensitiveDataLogging()
            .UseInMemoryDatabase("GrillBot");

        return new DatabaseContext(builder.Options);
    }

    public override GrillBotRepository CreateRepository()
    {
        Context = CreateContext();
        return new GrillBotRepository(Context, TestServices.CounterManager.Value);
    }

    public static void ClearDatabase()
    {
        var context = CreateContext();
        ClearDatabase(context);
    }

    private static void ClearDatabase(DbContext context)
    {
        context.ChangeTracker.Clear();
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
    }
}
