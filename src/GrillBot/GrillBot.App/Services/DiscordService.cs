using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GrillBot.App.Handlers;
using GrillBot.App.Infrastructure.TypeReaders;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Logging;
using GrillBot.App.Services.Reminder;
using GrillBot.App.Services.Sync;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Database.Services;
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
        private IHostApplicationLifetime Lifetime { get; }

        public DiscordService(DiscordSocketClient client, IConfiguration configuration, IServiceProvider provider, CommandService commandService,
            LoggingService _, IHostApplicationLifetime hostApplicationLifetime)
        {
            DiscordSocketClient = client;
            Configuration = configuration;
            Provider = provider;
            CommandService = commandService;
            Lifetime = hostApplicationLifetime;

            DiscordSocketClient.Log += OnLogAsync;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var token = Configuration.GetValue<string>("Discord:Token");

            InitServices();
            DiscordSocketClient.Ready += () => DiscordSocketClient.SetStatusAsync(UserStatus.Online);

            await DiscordSocketClient.LoginAsync(TokenType.Bot, token);
            await DiscordSocketClient.StartAsync();
            await DiscordSocketClient.SetStatusAsync(UserStatus.Idle);

            CommandService.AddTypeReader<Guid>(new GuidTypeReader());
            CommandService.AddTypeReader<IMessage>(new MessageTypeReader(), true);
            CommandService.AddTypeReader<IEmote>(new EmotesTypeReader());
            CommandService.AddTypeReader<IUser>(new UserTypeReader(), true);
            CommandService.AddTypeReader<DateTime>(new DateTimeTypeReader(), true);
            CommandService.AddTypeReader<bool>(new BooleanTypeReader(), true);

            await CommandService.AddModulesAsync(Assembly.GetEntryAssembly(), Provider);
        }

        private void InitServices()
        {
            var services = new[]
            {
                typeof(DiscordSyncService), typeof(AutoReplyService), typeof(InviteService), typeof(AuditLogService),
                typeof(PointsService), typeof(EmoteChainService),
                typeof(BoosterService), typeof(RemindService), typeof(MessageCache.MessageCache),
                typeof(ChannelService), typeof(CommandHandler), typeof(EmoteService), typeof(SearchingService),
                typeof(ReactionHandler)
            };

            foreach (var service in services) Provider.GetRequiredService(service);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await DiscordSocketClient.StopAsync();
            await DiscordSocketClient.LogoutAsync();
        }

        private async Task OnLogAsync(LogMessage message)
        {
            if (message.Source != "Gateway" || message.Exception == null) return;
            if (message.Exception is GatewayReconnectException && message.Exception.Message.StartsWith("Server missed last heartbeat", StringComparison.InvariantCultureIgnoreCase))
            {
                var auditLogItem = AuditLogItem.Create(AuditLogItemType.Info, null, null, DiscordSocketClient.CurrentUser, "Restart aplikace po odpojení.");
                var dbFactory = Provider.GetRequiredService<GrillBotContextFactory>();
                using var dbContext = dbFactory.Create();
                await dbContext.InitUserAsync(DiscordSocketClient.CurrentUser, CancellationToken.None);
                await dbContext.AddAsync(auditLogItem);

                await Task.WhenAll(
                    dbContext.SaveChangesAsync(),
                    Task.Delay(5000)
                );

                Environment.Exit(1);
            }
        }
    }
}
