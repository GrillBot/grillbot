using GrillBot.App.Infrastructure.Jobs;
using GrillBot.App.Services.AuditLog;
using GrillBot.Cache.Services.Managers;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Managers;
using GrillBot.Common.Managers.Logging;
using Quartz;

namespace GrillBot.App.Services;

[DisallowConcurrentExecution]
[DisallowUninitialized]
public class MessageCacheJob : Job
{
    private IMessageCacheManager MessageCacheManager { get; }

    public MessageCacheJob(AuditLogWriter auditLogWriter, IDiscordClient discordClient, InitManager initManager, IMessageCacheManager messageCacheManager, LoggingManager loggingManager) : base(
        auditLogWriter, discordClient, initManager, loggingManager)
    {
        MessageCacheManager = messageCacheManager;
    }

    protected override async Task RunAsync(IJobExecutionContext context)
    {
        context.Result = await MessageCacheManager.ProcessScheduledTaskAsync();
    }
}
