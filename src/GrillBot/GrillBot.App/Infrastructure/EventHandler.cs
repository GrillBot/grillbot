using Discord.WebSocket;

namespace GrillBot.App.Infrastructure
{
    public abstract class EventHandler
    {
        protected DiscordSocketClient DiscordClient { get; }

        protected EventHandler(DiscordSocketClient client)
        {
            DiscordClient = client;
        }
    }
}
