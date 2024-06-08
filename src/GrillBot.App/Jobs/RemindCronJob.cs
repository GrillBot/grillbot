using GrillBot.App.Infrastructure.Jobs;
using GrillBot.Core.Services.RemindService;
using Quartz;

namespace GrillBot.App.Jobs;

[DisallowConcurrentExecution]
[DisallowUninitialized]
public class RemindCronJob : Job
{
    private readonly IRemindServiceClient _remindService;

    public RemindCronJob(IServiceProvider serviceProvider, IRemindServiceClient remindService) : base(serviceProvider)
    {
        _remindService = remindService;
    }

    protected override async Task RunAsync(IJobExecutionContext context)
    {
        var result = await _remindService.ProcessPendingRemindersAsync();

        if (result.RemindersCount > 0)
        {
            var resultBuilder = new StringBuilder($"Processed reminders ({result.RemindersCount}):").AppendLine()
                .AppendJoin("\n", result.Messages);

            context.Result = resultBuilder.ToString();
        }
    }
}
