using AutoMapper;
using GrillBot.App.Services.Discord;

namespace GrillBot.App.Infrastructure;

/// <summary>
/// Base class for all singleton services that uses event handlers (message received, invite, ...) or database.
/// </summary>
public abstract class ServiceBase
{
    protected DiscordSocketClient DiscordClient { get; }
    protected IDiscordClient DcClient { get; }
    protected GrillBotContextFactory DbFactory { get; }
    protected DiscordInitializationService InitializationService { get; }
    protected IMapper Mapper { get; }

    protected ServiceBase(DiscordSocketClient client, GrillBotContextFactory dbFactory = null,
        DiscordInitializationService initializationService = null, IDiscordClient dcClient = null,
        IMapper mapper = null)
    {
        DiscordClient = client;
        DbFactory = dbFactory;
        InitializationService = initializationService;
        DcClient = dcClient;
        Mapper = mapper;
    }

    protected GrillBotContext CreateContext()
        => DbFactory.Create();

    protected async Task<bool> CheckPendingMigrationsAsync()
    {
        using var context = DbFactory.Create();

        return (await context.Database.GetPendingMigrationsAsync()).Any();
    }
}
