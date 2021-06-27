using Discord;
using GrillBot.App.Services.CronJobs;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GrillBot.App.Services.Reminder
{
    public class RemindCronJob : CronJobTask
    {
        private RemindService RemindService { get; }
        private LoggingService LoggingService { get; }

        public RemindCronJob(IConfiguration configuration, RemindService remindService, LoggingService logging)
            : base(configuration.GetValue<string>("Reminder:CronJob"))
        {
            RemindService = remindService;
            LoggingService = logging;
        }

        public override async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            var reminders = await RemindService.GetProcessableReminderIdsAsync();

            foreach (var id in reminders)
            {
                try
                {
                    await RemindService.ProcessRemindFromJobAsync(id);
                }
                catch (Exception ex)
                {
                    var logMessage = new LogMessage(LogSeverity.Error, nameof(RemindCronJob), $"An error occured in remind processing #{id}", ex);

                    await LoggingService.OnLogAsync(logMessage);
                }
            }
        }
    }
}
