using Discord.WebSocket;

namespace GrillBot.App.Infrastructure
{
    /// <summary>
    /// Base class for all singleton services that uses event handlers (message received, invite, ...).
    /// </summary>
    public abstract class ServiceBase
    {
        protected DiscordSocketClient DiscordClient { get; }

        protected ServiceBase(DiscordSocketClient client)
        {
            DiscordClient = client;
        }
    }
}
