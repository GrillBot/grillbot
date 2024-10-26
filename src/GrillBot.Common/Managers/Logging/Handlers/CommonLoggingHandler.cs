using Discord;
using Microsoft.Extensions.Logging;

namespace GrillBot.Common.Managers.Logging.Handlers;

public class CommonLoggerHandler : ILoggingHandler
{
    private readonly ILoggerFactory _factory;

    public CommonLoggerHandler(ILoggerFactory factory)
    {
        _factory = factory;
    }

    public Task<bool> CanHandleAsync(LogSeverity severity, string source, Exception? exception = null)
    {
        return Task.FromResult(source != "API");
    }

    public Task InfoAsync(string source, string message)
    {
        CreateLogger(source).LogInformation("{message}", message);
        return Task.CompletedTask;
    }

    public Task WarningAsync(string source, string message, Exception? exception = null)
    {
        var logger = CreateLogger(source);

        if (exception == null)
            logger.LogWarning("{message}", message);
        else
            logger.LogWarning(exception, "{message}", message);
        return Task.CompletedTask;
    }

    public Task ErrorAsync(string source, string message, Exception exception)
    {
        CreateLogger(source).LogError(exception, "{message}", message);
        return Task.CompletedTask;
    }

    private ILogger CreateLogger(string source) => _factory.CreateLogger(source);
}
