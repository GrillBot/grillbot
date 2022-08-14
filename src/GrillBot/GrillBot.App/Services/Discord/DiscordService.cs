using Discord.Commands;
using Discord.Interactions;
using Discord.Net;
using GrillBot.App.Infrastructure;
using GrillBot.App.Infrastructure.TypeReaders;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Logging;
using GrillBot.Common.Managers;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Reflection;

namespace GrillBot.App.Services.Discord;

public class DiscordService : IHostedService
{
    private DiscordSocketClient DiscordSocketClient { get; }
    private IConfiguration Configuration { get; }
    private IServiceProvider Provider { get; }
    private CommandService CommandService { get; }
    private IWebHostEnvironment Environment { get; }
    private InitManager InitManager { get; }
    private LoggingService LoggingService { get; }
    private InteractionService InteractionService { get; }
    private AuditLogWriter AuditLogWriter { get; }

    public DiscordService(DiscordSocketClient client, IConfiguration configuration, IServiceProvider provider, CommandService commandService,
        LoggingService loggingService, IWebHostEnvironment webHostEnvironment, InitManager initManager, InteractionService interactionService,
        AuditLogWriter auditLogWriter, EventManager _)
    {
        DiscordSocketClient = client;
        Configuration = configuration;
        Provider = provider;
        CommandService = commandService;
        Environment = webHostEnvironment;
        InitManager = initManager;
        LoggingService = loggingService;
        InteractionService = interactionService;
        AuditLogWriter = auditLogWriter;

        DiscordSocketClient.Log += OnLogAsync;
        CommandService.Log += OnLogAsync;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var token = Configuration.GetValue<string>("Discord:Token");

        InitServices();
        DiscordSocketClient.Ready += async () =>
        {
            if (InteractionService.Modules.Count > 0)
            {
                foreach (var guild in DiscordSocketClient.Guilds)
                {
                    try
                    {
                        await InteractionService.RegisterCommandsToGuildAsync(guild.Id);
                    }
                    catch (HttpException ex) when (ex.DiscordCode == DiscordErrorCode.MissingOAuth2Scope)
                    {
                        await LoggingService.ErrorAsync("Event(Ready)", $"Guild {guild.Name} not have OAuth2 scope for interaction registration.", ex);
                    }
                }
            }

            InitManager.Set(true);
        };

        CommandService.RegisterTypeReaders();
        InteractionService.RegisterTypeConverters();

        var assembly = Assembly.GetEntryAssembly();
        await CommandService.AddModulesAsync(assembly, Provider);
        await InteractionService.AddModulesAsync(assembly, Provider);

        await DiscordSocketClient.LoginAsync(TokenType.Bot, token);
        await DiscordSocketClient.StartAsync();
    }

    private void InitServices()
    {
        var currentAssembly = Assembly.GetExecutingAssembly();

        currentAssembly
            .GetTypes()
            .Where(o => o.Assembly == currentAssembly && o.IsClass && !o.IsAbstract && o.GetCustomAttribute<InitializableAttribute>() != null)
            .OrderBy(o => o.Name)
            .ToList()
            .ForEach(service => Provider.GetRequiredService(service));
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await DiscordSocketClient.StopAsync();
        await DiscordSocketClient.LogoutAsync();
    }

    private async Task OnLogAsync(LogMessage message)
    {
        if (message.Source != "Gateway" || message.Exception == null || Environment.IsDevelopment()) return;
        if (message.Exception is GatewayReconnectException && message.Exception.Message.StartsWith("Server missed last heartbeat", StringComparison.InvariantCultureIgnoreCase))
        {
            var item = new AuditLogDataWrapper(AuditLogItemType.Info, "Application restart after reconnect", null, null, DiscordSocketClient.CurrentUser);
            await AuditLogWriter.StoreAsync(item);

            await Task.Delay(3000);
            Process.GetCurrentProcess().Kill();
        }
    }
}
