using GrillBot.App.Infrastructure.Jobs;
using GrillBot.Core.Services.RemindService;
using Quartz;

namespace GrillBot.App.Jobs;

[DisallowConcurrentExecution]
[DisallowUninitialized]
public class RemindCronJob(
    IServiceProvider serviceProvider,
    IRemindServiceClient _remindClient
) : Job(serviceProvider)
{
    protected override async Task RunAsync(IJobExecutionContext context)
    {
        var result = await _remindClient.ProcessPendingRemindersAsync();
        if (result.RemindersCount > 0)
        {
            var resultBuilder = new StringBuilder($"Processed reminders ({result.RemindersCount}):").AppendLine()
                .AppendJoin("\n", result.Messages);

            context.Result = resultBuilder.ToString();
        }
    }
}
