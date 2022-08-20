using GrillBot.App.Infrastructure.Jobs;
using GrillBot.App.Services.AuditLog;
using GrillBot.Common.Managers;
using GrillBot.Common.Managers.Logging;
using Quartz;

namespace GrillBot.App.Services.Unverify;

[DisallowConcurrentExecution]
[DisallowUninitialized]
public class UnverifyCronJob : Job
{
    private UnverifyService UnverifyService { get; }

    public UnverifyCronJob(AuditLogWriter auditLogWriter, IDiscordClient discordClient, UnverifyService unverifyService, InitManager initManager, LoggingManager loggingManager) : base(auditLogWriter,
        discordClient, initManager, loggingManager)
    {
        UnverifyService = unverifyService;
    }

    protected override async Task RunAsync(IJobExecutionContext context)
    {
        var pending = await UnverifyService.GetPendingUnverifiesForRemoveAsync();

        foreach (var (guildId, userId) in pending)
            await UnverifyService.UnverifyAutoremoveAsync(guildId, userId);

        if (pending.Count > 0)
            context.Result = $"Processed: {string.Join(", ", pending.Select(o => $"{o.Item1}/{o.Item2}"))}";
    }
}
