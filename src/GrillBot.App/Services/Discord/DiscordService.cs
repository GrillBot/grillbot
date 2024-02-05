using Discord.Interactions;
using GrillBot.App.Infrastructure.TypeReaders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Reflection;
using GrillBot.App.Handlers;
using GrillBot.App.Managers;
using GrillBot.Common.Managers.Events;
using GrillBot.Common.Managers.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Net.WebSockets;
using System.Net.Sockets;

namespace GrillBot.App.Services.Discord;

public class DiscordService : IHostedService
{
    private DiscordSocketClient DiscordSocketClient { get; }
    private IConfiguration Configuration { get; }
    private IServiceProvider Provider { get; }
    private IWebHostEnvironment Environment { get; }
    private InteractionService InteractionService { get; }

    public DiscordService(DiscordSocketClient client, IConfiguration configuration, IServiceProvider provider, IWebHostEnvironment webHostEnvironment, InteractionService interactionService)
    {
        DiscordSocketClient = client;
        Configuration = configuration;
        Provider = provider;
        Environment = webHostEnvironment;
        InteractionService = interactionService;

        DiscordSocketClient.Log += OnLogAsync;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        InitServices();

        var token = Configuration.GetValue<string>("Discord:Token");

        InteractionService.AddTypeConverter<DateTime>(new DateTimeTypeConverter());
        InteractionService.AddTypeConverter<IEmote>(new EmotesTypeConverter());
        InteractionService.AddTypeConverter<IMessage>(new MessageTypeConverter());
        InteractionService.AddTypeConverter<IEnumerable<IUser>>(new UsersTypeConverter());

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
        Provider.GetRequiredService<InteractionHandler>();
        Provider.GetRequiredService<PinManager>();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await DiscordSocketClient.StopAsync();
        await DiscordSocketClient.LogoutAsync();
    }

    private async Task OnLogAsync(LogMessage message)
    {
        if (message.Source != "Gateway" || message.Exception == null || Environment.IsDevelopment())
            return;

        /*
         * Shutdown bot while network errors.
         * 1) Server missed last heartbeat - Caused due to slow network connection.
         * 2) Bot is not able keep stable websocket connection (Broken pipe) - Caused due to network outages (probably).
         */
        var canShutdown = (message.Exception is GatewayReconnectException && message.Exception.Message.StartsWith("Server missed last heartbeat", StringComparison.InvariantCultureIgnoreCase)) ||
            (message.Exception is WebSocketException && message.Exception.InnerException is IOException && message.Exception.InnerException.InnerException is SocketException ex && (ex.NativeErrorCode == 32 || ex.ErrorCode == 32));

        if (canShutdown)
        {
            await Task.Delay(3000);
            Process.GetCurrentProcess().Kill();
        }
    }
}
