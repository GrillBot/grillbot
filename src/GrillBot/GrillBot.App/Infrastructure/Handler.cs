using Discord.WebSocket;

namespace GrillBot.App.Infrastructure
{
    public abstract class Handler
    {
        protected DiscordSocketClient DiscordClient { get; }

        protected Handler(DiscordSocketClient client)
        {
            DiscordClient = client;
        }
    }
}
