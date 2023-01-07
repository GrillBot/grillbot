using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.Common.Managers.Logging;

public class LoggingManager
{
    private DiscordSocketClient DiscordClient { get; }
    private CommandService CommandService { get; }
    private InteractionService InteractionService { get; }
    private IServiceProvider ServiceProvider { get; }

    public LoggingManager(DiscordSocketClient discordClient, CommandService commandService, InteractionService interactionService, IServiceProvider serviceProvider)
    {
        DiscordClient = discordClient;
        ServiceProvider = serviceProvider;
        CommandService = commandService;
        InteractionService = interactionService;

        DiscordClient.Log += OnLogAsync;
        InteractionService.Log += OnLogAsync;
        CommandService.Log += OnLogAsync;
    }

    private async Task OnLogAsync(LogMessage message)
    {
        foreach (var handler in ServiceProvider.GetServices<ILoggingHandler>())
        {
            if (!await handler.CanHandleAsync(message.Severity, message.Source, message.Exception))
                continue;

            switch (message.Severity)
            {
                case LogSeverity.Critical or LogSeverity.Error:
                    await handler.ErrorAsync(message.Source, message.Message, message.Exception);
                    break;
                case LogSeverity.Debug or LogSeverity.Info or LogSeverity.Verbose:
                    await handler.InfoAsync(message.Source, message.Message);
                    break;
                case LogSeverity.Warning:
                    await handler.WarningAsync(message.Source, message.Message, message.Exception);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(message));
            }
        }
    }

    public Task InfoAsync(string source, string message)
        => OnLogAsync(new LogMessage(LogSeverity.Info, source, message));

    public Task ErrorAsync(string source, string message, Exception exception)
        => OnLogAsync(new LogMessage(LogSeverity.Error, source, message, exception));
}
