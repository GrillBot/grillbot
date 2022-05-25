using GrillBot.App.Infrastructure.Jobs;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Logging;
using GrillBot.Common.Managers;
using Quartz;

namespace GrillBot.App.Services.MessageCache;

[DisallowConcurrentExecution]
[DisallowUninitialized]
public class MessageCacheCheckCron : Job
{
    private MessageCache MessageCache { get; }

    public MessageCacheCheckCron(LoggingService loggingService, AuditLogService auditLogService, IDiscordClient discordClient,
        MessageCache messageCache, InitManager initManager) : base(loggingService, auditLogService, discordClient, initManager)
    {
        MessageCache = messageCache;
    }

    public override async Task RunAsync(IJobExecutionContext context)
    {
        context.Result = await MessageCache.RunCheckAsync(context.CancellationToken);
    }
}
