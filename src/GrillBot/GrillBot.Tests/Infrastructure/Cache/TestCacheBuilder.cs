using GrillBot.Cache.Services;
using GrillBot.Cache.Services.Repository;
using GrillBot.Common.Managers.Counters;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace GrillBot.Tests.Infrastructure.Cache;

[ExcludeFromCodeCoverage]
public class TestCacheBuilder : GrillBotCacheBuilder
{
    private GrillBotCacheContext Context { get; set; }

    public TestCacheBuilder() : base(DiHelper.CreateEmptyProvider())
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
        return new GrillBotCacheRepository(Context, new CounterManager());
    }

    public void ClearDatabase()
    {
        Context.ChangeTracker.Clear();
        Context.RemoveRange(Context.DirectApiMessages.AsEnumerable());
        Context.RemoveRange(Context.MessageIndex.AsEnumerable());
        Context.RemoveRange(Context.ProfilePictures.AsEnumerable());
        Context.SaveChanges();
    }
}
