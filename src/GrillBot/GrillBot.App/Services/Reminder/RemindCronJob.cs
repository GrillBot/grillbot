using GrillBot.App.Services.Logging;
using Quartz;

namespace GrillBot.App.Services.Reminder
{
    [DisallowConcurrentExecution]
    public class RemindCronJob : IJob
    {
        private RemindService RemindService { get; }
        private LoggingService LoggingService { get; }

        public RemindCronJob(RemindService remindService, LoggingService logging)
        {
            RemindService = remindService;
            LoggingService = logging;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var reminders = await RemindService.GetProcessableReminderIdsAsync();
            await LoggingService.InfoAsync("RemindCron", $"Triggered remind processing job at {DateTime.Now}. Reminders to process {reminders.Count}");

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
