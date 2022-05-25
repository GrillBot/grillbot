using GrillBot.App.Infrastructure.Jobs;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Logging;
using GrillBot.Common.Managers;
using Quartz;

namespace GrillBot.App.Services.Reminder;

[DisallowConcurrentExecution]
[DisallowUninitialized]
public class RemindCronJob : Job
{
    private RemindService RemindService { get; }

    public RemindCronJob(LoggingService loggingService, AuditLogService auditLogService, IDiscordClient discordClient,
        RemindService remindService, InitManager initManager) : base(loggingService, auditLogService, discordClient, initManager)
    {
        RemindService = remindService;
    }

    public override async Task RunAsync(IJobExecutionContext context)
    {
        var reminders = await RemindService.GetProcessableReminderIdsAsync(context.CancellationToken);

        foreach (var id in reminders)
        {
            await RemindService.ProcessRemindFromJobAsync(id);
        }

        context.Result = $"Reminders: {reminders.Count} ({string.Join(", ", reminders)})";
    }
}
