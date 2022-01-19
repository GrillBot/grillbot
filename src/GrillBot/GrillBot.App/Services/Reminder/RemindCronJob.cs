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
            await LoggingService.InfoAsync("RemindCron", $"Trigged remind processing job at {DateTime.Now}");
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
