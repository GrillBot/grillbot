using Discord.Interactions;
using GrillBot.App.Infrastructure;
using GrillBot.App.Infrastructure.TypeReaders;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Reflection;
using GrillBot.App.Managers;
using GrillBot.Common.Managers.Events;
using GrillBot.Common.Managers.Logging;
using GrillBot.Data.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Services.Discord;

public class DiscordService : IHostedService
{
    private DiscordSocketClient DiscordSocketClient { get; }
    private IConfiguration Configuration { get; }
    private IServiceProvider Provider { get; }
    private IWebHostEnvironment Environment { get; }
    private InteractionService InteractionService { get; }
    private AuditLogWriteManager AuditLogWriteManager { get; }

    public DiscordService(DiscordSocketClient client, IConfiguration configuration, IServiceProvider provider, IWebHostEnvironment webHostEnvironment, InteractionService interactionService,
        AuditLogWriteManager auditLogWriteManager)
    {
        DiscordSocketClient = client;
        Configuration = configuration;
        Provider = provider;
        Environment = webHostEnvironment;
        InteractionService = interactionService;
        AuditLogWriteManager = auditLogWriteManager;

        DiscordSocketClient.Log += OnLogAsync;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        InitServices();

        var token = Configuration.GetValue<string>("Discord:Token");
        InteractionService.RegisterTypeConverters();

        var assembly = Assembly.GetEntryAssembly();
        await InteractionService.AddModulesAsync(assembly, Provider);
        await DiscordSocketClient.LoginAsync(TokenType.Bot, token);
        await DiscordSocketClient.StartAsync();
    }

    private void InitServices()
    {
        Provider.GetRequiredService<LoggingManager>();
        Provider.GetRequiredService<EventLogManager>();
        Provider.GetRequiredService<EventManager>();

        var currentAssembly = Assembly.GetExecutingAssembly();
        var initializable = currentAssembly
            .GetTypes()
            .Where(o => o.Assembly == currentAssembly && o is { IsClass: true, IsAbstract: false } && o.GetCustomAttribute<InitializableAttribute>() != null)
            .OrderBy(o => o.Name)
            .ToList();

        foreach (var service in initializable)
        {
            var dependency = Provider.GetService(service);
            if (dependency != null) continue;

            var interfaces = service.GetInterfaces();
            if (interfaces.Length > 0)
            {
                foreach (var @interface in interfaces)
                {
                    dependency = Provider.GetService(@interface);
                    if (dependency != null) break;
                }
            }

            if (dependency == null)
                throw new GrillBotException($"Failed initialization of service {service.FullName}");
        }
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
            await AuditLogWriteManager.StoreAsync(item);

            await Task.Delay(3000);
            Process.GetCurrentProcess().Kill();
        }
    }
}
