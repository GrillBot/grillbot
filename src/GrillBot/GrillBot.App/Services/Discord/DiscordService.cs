﻿using Discord.Commands;
using Discord.Interactions;
using Discord.Net;
using GrillBot.App.Infrastructure;
using GrillBot.App.Infrastructure.TypeReaders;
using GrillBot.App.Services.AuditLog;
using GrillBot.Common.Managers;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Reflection;
using GrillBot.Common.Managers.Logging;
using GrillBot.Data.Exceptions;

namespace GrillBot.App.Services.Discord;

public class DiscordService : IHostedService
{
    private DiscordSocketClient DiscordSocketClient { get; }
    private IConfiguration Configuration { get; }
    private IServiceProvider Provider { get; }
    private CommandService CommandService { get; }
    private IWebHostEnvironment Environment { get; }
    private InitManager InitManager { get; }
    private InteractionService InteractionService { get; }
    private AuditLogWriter AuditLogWriter { get; }
    private LoggingManager LoggingManager { get; }

    public DiscordService(DiscordSocketClient client, IConfiguration configuration, IServiceProvider provider, CommandService commandService,
        IWebHostEnvironment webHostEnvironment, InitManager initManager, InteractionService interactionService, AuditLogWriter auditLogWriter, EventLogManager _, LoggingManager loggingManager)
    {
        DiscordSocketClient = client;
        Configuration = configuration;
        Provider = provider;
        CommandService = commandService;
        Environment = webHostEnvironment;
        InitManager = initManager;
        InteractionService = interactionService;
        AuditLogWriter = auditLogWriter;
        LoggingManager = loggingManager;

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
                        await LoggingManager.ErrorAsync("Event(Ready)", $"Guild {guild.Name} not have OAuth2 scope for interaction registration.", ex);
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

        var initializable = currentAssembly
            .GetTypes()
            .Where(o => o.Assembly == currentAssembly && o.IsClass && !o.IsAbstract && o.GetCustomAttribute<InitializableAttribute>() != null)
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
                    if (dependency != null) continue;
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
            await AuditLogWriter.StoreAsync(item);

            await Task.Delay(3000);
            Process.GetCurrentProcess().Kill();
        }
    }
}
