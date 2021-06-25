using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GrillBot.App.Handlers;
using GrillBot.App.Infrastructure.TypeReaders;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Sync;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace GrillBot.App.Services
{
    public class DiscordService : IHostedService
    {
        private DiscordSocketClient DiscordSocketClient { get; }
        private IConfiguration Configuration { get; }
        private IServiceProvider Provider { get; }
        private CommandService CommandService { get; }

        public DiscordService(DiscordSocketClient client, IConfiguration configuration, IServiceProvider provider, CommandService commandService,
            LoggingService _)
        {
            DiscordSocketClient = client;
            Configuration = configuration;
            Provider = provider;
            CommandService = commandService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var token = Configuration.GetValue<string>("Discord:Token");

            InitServices();
            await DiscordSocketClient.LoginAsync(TokenType.Bot, token);
            await DiscordSocketClient.StartAsync();

            CommandService.AddTypeReader<Guid>(new GuidTypeReader());
            CommandService.AddTypeReader<IMessage>(new MessageTypeReader(), true);
            CommandService.AddTypeReader<IEmote>(new EmotesTypeReader());

            await CommandService.AddModulesAsync(Assembly.GetEntryAssembly(), Provider);
        }

        private void InitServices()
        {
            var services = new[]
            {
                typeof(MessageCache.MessageCache), typeof(AutoReplyService), typeof(ChannelService), typeof(InviteService),
                typeof(CommandHandler), typeof(ReactionHandler), typeof(AuditLogService), typeof(PointsService),
                typeof(EmoteService), typeof(DiscordSyncService), typeof(EmoteChainService), typeof(SearchingService)
            };

            foreach (var service in services) Provider.GetRequiredService(service);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await DiscordSocketClient.StopAsync();
            await DiscordSocketClient.LogoutAsync();
        }
    }
}
