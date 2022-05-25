using AutoMapper;
using GrillBot.Cache.Services;

namespace GrillBot.App.Infrastructure;

/// <summary>
/// Base class for all singleton services that uses event handlers (message received, invite, ...) or database.
/// </summary>
public abstract class ServiceBase
{
    protected DiscordSocketClient DiscordClient { get; }
    protected IDiscordClient DcClient { get; }
    protected GrillBotContextFactory DbFactory { get; }
    protected IMapper Mapper { get; }
    protected GrillBotCacheBuilder CacheBuilder { get; }

    protected ServiceBase(DiscordSocketClient client, GrillBotContextFactory dbFactory = null, IDiscordClient dcClient = null,
        IMapper mapper = null, GrillBotCacheBuilder cacheBuilder = null)
    {
        DiscordClient = client;
        DbFactory = dbFactory;
        DcClient = dcClient;
        Mapper = mapper;
        CacheBuilder = cacheBuilder;
    }

    protected GrillBotContext CreateContext()
        => DbFactory.Create();

    protected async Task<bool> CheckPendingMigrationsAsync()
    {
        using var context = DbFactory.Create();

        return (await context.Database.GetPendingMigrationsAsync()).Any();
    }
}
