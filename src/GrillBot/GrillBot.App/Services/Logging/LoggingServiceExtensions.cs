using Discord;
using System;
using System.Threading.Tasks;

namespace GrillBot.App.Services.Logging
{
    public static class LoggingServiceExtensions
    {
        public static Task TriggerAsync(this LoggingService service, LogSeverity severity, string source, string message, Exception exception = null)
        {
            var msg = new LogMessage(severity, source, message, exception);
            return service.OnLogAsync(msg);
        }

        public static Task ErrorAsync(this LoggingService service, string source, string message, Exception exception)
            => TriggerAsync(service, LogSeverity.Error, source, message, exception);

        public static Task InfoAsync(this LoggingService service, string source, string message)
            => TriggerAsync(service, LogSeverity.Info, source, message, null);
    }
}
