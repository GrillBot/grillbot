using GrillBot.Cache.Services;
using GrillBot.Cache.Services.Repository;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace GrillBot.Tests.Infrastructure.Cache;

[ExcludeFromCodeCoverage]
public class TestCacheBuilder : GrillBotCacheBuilder
{
    private GrillBotCacheContext Context { get; set; }

    public TestCacheBuilder() : base(null!)
    {
    }

    private static GrillBotCacheContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<GrillBotCacheContext>()
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging()
            .EnableThreadSafetyChecks()
            .UseInMemoryDatabase("GrillBot-Cache")
            .Options;

        return new GrillBotCacheContext(options);
    }

    public override GrillBotCacheRepository CreateRepository()
    {
        Context = CreateContext();
        return new GrillBotCacheRepository(Context, TestServices.CounterManager.Value);
    }

    public static void ClearDatabase()
    {
        var context = CreateContext();
        ClearDatabase(context);
    }

    private static void ClearDatabase(GrillBotCacheContext context)
    {
        context.ChangeTracker.Clear();
        context.RemoveRange(context.DirectApiMessages.AsEnumerable());
        context.RemoveRange(context.MessageIndex.AsEnumerable());
        context.RemoveRange(context.ProfilePictures.AsEnumerable());
        context.SaveChanges();
    }
}
