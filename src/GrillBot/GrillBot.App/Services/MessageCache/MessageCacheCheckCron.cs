using Discord;
using GrillBot.App.Services.CronJobs;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GrillBot.App.Services.MessageCache
{
    public class MessageCacheCheckCron : CronJobTask
    {
        private MessageCache MessageCache { get; }
        private LoggingService LoggingService { get; }

        public MessageCacheCheckCron(IConfiguration configuration, LoggingService loggingService, MessageCache messageCache)
            : base(configuration["Discord:MessageCache:Cron"])
        {
            LoggingService = loggingService;
            MessageCache = messageCache;
        }

        public override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            try
            {
                await MessageCache.RunCheckAsync();
            }
            catch (Exception ex)
            {
                var msg = new LogMessage(LogSeverity.Error, nameof(MessageCacheCheckCron), "An error occured when message cache check processing.", ex);
                await LoggingService.OnLogAsync(msg);
            }
        }
    }
}
