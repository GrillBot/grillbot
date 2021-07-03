using Discord.WebSocket;
using GrillBot.App.Services.CronJobs;
using GrillBot.App.Services.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GrillBot.App.Services.Reminder
{
    public class RemindCronJob : CronJobTask
    {
        private RemindService RemindService { get; }
        private LoggingService LoggingService { get; }

        public RemindCronJob(IConfiguration configuration, RemindService remindService, LoggingService logging,
            DiscordSocketClient discordClient) : base(configuration.GetValue<string>("Reminder:CronJob"), discordClient)
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
                    await LoggingService.ErrorAsync("RemindCron", $"An error occured in remind processing {id}", ex);
                }
            }
        }
    }
}
