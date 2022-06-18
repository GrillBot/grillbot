﻿using GrillBot.App.Infrastructure.Jobs;
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

    public RemindCronJob(LoggingService loggingService, AuditLogWriter auditLogWriter, IDiscordClient discordClient,
        RemindService remindService, InitManager initManager) : base(loggingService, auditLogWriter, discordClient, initManager)
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
