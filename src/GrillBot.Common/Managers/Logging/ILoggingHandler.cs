using Discord;

namespace GrillBot.Common.Managers.Logging;

public interface ILoggingHandler
{
    Task<bool> CanHandleAsync(LogSeverity severity, string source, Exception? exception = null);
    
    Task InfoAsync(string source, string message);
    Task WarningAsync(string source, string message, Exception? exception = null);
    Task ErrorAsync(string source, string message, Exception exception);
}
