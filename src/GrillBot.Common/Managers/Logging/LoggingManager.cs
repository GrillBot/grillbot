using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.Common.Managers.Logging;

public class LoggingManager
{
    private readonly IServiceProvider _serviceProvider;

    public LoggingManager(DiscordSocketClient discordClient, InteractionService interactionService, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        interactionService.Log += OnLogAsync;
        discordClient.Log += OnLogAsync;
    }

    private async Task OnLogAsync(LogMessage message)
    {
        using var scope = _serviceProvider.CreateScope();

        foreach (var handler in scope.ServiceProvider.GetServices<ILoggingHandler>())
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

    public Task WarningAsync(string source, string message, Exception? exception = null)
        => OnLogAsync(new LogMessage(LogSeverity.Warning, source, message, exception));
}
