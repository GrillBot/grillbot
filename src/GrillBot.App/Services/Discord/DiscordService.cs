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
        if (message.Source != "Gateway" || message.Exception == null || Environment.IsDevelopment()) return;
        if (message.Exception is GatewayReconnectException && message.Exception.Message.StartsWith("Server missed last heartbeat", StringComparison.InvariantCultureIgnoreCase))
        {
            await Task.Delay(3000);
            Process.GetCurrentProcess().Kill();
        }
    }
}
