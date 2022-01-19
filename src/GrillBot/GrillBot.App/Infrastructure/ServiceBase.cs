using GrillBot.App.Services.Discord;

namespace GrillBot.App.Infrastructure
{
    /// <summary>
    /// Base class for all singleton services that uses event handlers (message received, invite, ...) or database.
    /// </summary>
    public abstract class ServiceBase
    {
        protected DiscordSocketClient DiscordClient { get; }
        protected GrillBotContextFactory DbFactory { get; }
        protected DiscordInitializationService InitializationService { get; }

        protected ServiceBase(DiscordSocketClient client, GrillBotContextFactory dbFactory = null,
            DiscordInitializationService initializationService = null)
        {
            DiscordClient = client;
            DbFactory = dbFactory;
            InitializationService = initializationService;
        }
    }
}
