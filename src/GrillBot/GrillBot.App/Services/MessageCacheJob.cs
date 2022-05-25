using GrillBot.App.Infrastructure.Jobs;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Logging;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Managers;
using Quartz;

namespace GrillBot.App.Services;

[DisallowConcurrentExecution]
[DisallowUninitialized]
public class MessageCacheJob : Job
{
    private MessageCacheManager MessageCacheManager { get; }

    public MessageCacheJob(LoggingService loggingService, AuditLogService auditLogService,
        IDiscordClient discordClient, InitManager initManager, MessageCacheManager messageCacheManager)
        : base(loggingService, auditLogService, discordClient, initManager)
    {
        MessageCacheManager = messageCacheManager;
    }

    public override async Task RunAsync(IJobExecutionContext context)
    {
        context.Result = await MessageCacheManager.ProcessScheduledTaskAsync();
    }
}
