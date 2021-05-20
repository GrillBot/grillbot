using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace GrillBot.App.Services
{
    public class DiscordService : IHostedService
    {
        private DiscordSocketClient DiscordSocketClient { get; }
        private IConfiguration Configuration { get; }

        public DiscordService(DiscordSocketClient client, IConfiguration configuration)
        {
            DiscordSocketClient = client;
            Configuration = configuration;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var token = Configuration.GetConnectionString("Discord");

            await DiscordSocketClient.LoginAsync(TokenType.Bot, token);
            await DiscordSocketClient.StartAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await DiscordSocketClient.StopAsync();
            await DiscordSocketClient.LogoutAsync();
        }
    }
}
