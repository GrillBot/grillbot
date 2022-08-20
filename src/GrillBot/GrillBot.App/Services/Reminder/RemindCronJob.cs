using GrillBot.App.Infrastructure.Jobs;
using GrillBot.App.Services.AuditLog;
using GrillBot.Common.Managers;
using GrillBot.Common.Managers.Logging;
using Quartz;

namespace GrillBot.App.Services.Reminder;

[DisallowConcurrentExecution]
[DisallowUninitialized]
public class RemindCronJob : Job
{
    private RemindService RemindService { get; }

    public RemindCronJob(AuditLogWriter auditLogWriter, IDiscordClient discordClient, RemindService remindService, InitManager initManager, LoggingManager loggingManager) : base(auditLogWriter,
        discordClient, initManager, loggingManager)
    {
        RemindService = remindService;
    }

    protected override async Task RunAsync(IJobExecutionContext context)
    {
        var reminders = await RemindService.GetRemindIdsForProcessAsync();

        foreach (var id in reminders)
            await RemindService.ProcessRemindFromJobAsync(id);

        if (reminders.Count > 0)
            context.Result = $"Reminders: {reminders.Count} ({string.Join(", ", reminders)})";
    }
}
