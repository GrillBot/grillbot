using GrillBot.Cache.Services;
using GrillBot.Cache.Services.Repository;
using GrillBot.Common.Managers.Counters;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace GrillBot.Tests.Infrastructure.Cache;

[ExcludeFromCodeCoverage]
public class TestCacheBuilder : GrillBotCacheBuilder
{
    public TestCacheBuilder() : base(null)
    {
    }

    public static GrillBotCacheContext CreateContext()
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
        var context = CreateContext();
        return new GrillBotCacheRepository(context, new CounterManager());
    }

    public static void ClearDatabase(GrillBotCacheRepository repository)
    {
        repository.RemoveCollection(repository.DirectApiRepository.GetAll());
        repository.RemoveCollection(repository.MessageIndexRepository.GetMessagesAsync().Result);

        repository.Commit();
    }
}
