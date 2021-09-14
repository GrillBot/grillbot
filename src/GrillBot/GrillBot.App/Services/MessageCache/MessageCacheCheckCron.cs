using GrillBot.App.Services.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace GrillBot.App.Services.MessageCache
{
    [DisallowConcurrentExecution]
    public class MessageCacheCheckCron : IJob
    {
        private MessageCache MessageCache { get; }
        private LoggingService LoggingService { get; }

        public MessageCacheCheckCron(LoggingService loggingService, MessageCache messageCache)
        {
            LoggingService = loggingService;
            MessageCache = messageCache;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                await LoggingService.InfoAsync("MessageCacheCron", $"Triggered job at {DateTime.Now}");
                await MessageCache.RunCheckAsync();
            }
            catch (Exception ex)
            {
                await LoggingService.ErrorAsync("MessageCacheCron", "An error occured when message cache check processing.", ex);
            }
        }
    }
}
