using GrillBot.App.Infrastructure.Jobs;
using GrillBot.Cache.Services.Managers.MessageCache;
using Quartz;

namespace GrillBot.App.Jobs;

[DisallowConcurrentExecution]
[DisallowUninitialized]
public class MessageCacheJob(
    IServiceProvider serviceProvider,
    IMessageCacheManager _manager
) : Job(serviceProvider)
{
    protected override async Task RunAsync(IJobExecutionContext context)
    {
        context.Result = await _manager.ProcessScheduledTaskAsync();
    }
}
