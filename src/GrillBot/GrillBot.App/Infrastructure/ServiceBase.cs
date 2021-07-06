using Discord.WebSocket;
using GrillBot.Database.Services;

namespace GrillBot.App.Infrastructure
{
    /// <summary>
    /// Base class for all singleton services that uses event handlers (message received, invite, ...) or database.
    /// </summary>
    public abstract class ServiceBase
    {
        protected DiscordSocketClient DiscordClient { get; }
        protected GrillBotContextFactory DbFactory { get; }

        protected ServiceBase(DiscordSocketClient client, GrillBotContextFactory dbFactory = null)
        {
            DiscordClient = client;
            DbFactory = dbFactory;
        }
    }
}
