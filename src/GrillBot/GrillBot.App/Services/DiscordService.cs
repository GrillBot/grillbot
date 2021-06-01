using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GrillBot.App.Handlers;
using GrillBot.App.Infrastructure.TypeReaders;
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
            Provider.GetRequiredService<MessageCache>();
            Provider.GetRequiredService<AutoReplyService>();
            Provider.GetRequiredService<ChannelService>();
            Provider.GetRequiredService<InviteService>();
            Provider.GetRequiredService<CommandHandler>();
            Provider.GetRequiredService<ReactionHandler>();
            Provider.GetRequiredService<AuditLogService>();
            Provider.GetRequiredService<PointsService>();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await DiscordSocketClient.StopAsync();
            await DiscordSocketClient.LogoutAsync();
        }
    }
}
