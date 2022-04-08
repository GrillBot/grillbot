using GrillBot.App.Infrastructure.Jobs;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Discord;
using GrillBot.App.Services.Logging;
using Quartz;

namespace GrillBot.App.Services.Unverify;

[DisallowConcurrentExecution]
[DisallowUninitialized]
public class UnverifyCronJob : Job
{
    private UnverifyService UnverifyService { get; }

    public UnverifyCronJob(LoggingService loggingService, AuditLogService auditLogService, IDiscordClient discordClient,
        UnverifyService unverifyService, DiscordInitializationService initializationService)
        : base(loggingService, auditLogService, discordClient, initializationService)
    {
        UnverifyService = unverifyService;
    }

    public override async Task RunAsync(IJobExecutionContext context)
    {
        var pending = await UnverifyService.GetPendingUnverifiesForRemoveAsync(context.CancellationToken);

        foreach ((var guildId, var userId) in pending)
        {
            await UnverifyService.UnverifyAutoremoveAsync(guildId, userId);
        }

        if (pending.Count > 0)
            context.Result = $"Processed: {string.Join(", ", pending.Select(o => $"{o.Item1}/{o.Item2}"))}";
    }
}
