using GrillBot.App.Infrastructure.Jobs;
using GrillBot.Cache.Services.Managers.MessageCache;
using Quartz;

namespace GrillBot.App.Jobs;

[DisallowConcurrentExecution]
[DisallowUninitialized]
public class MessageCacheJob(IServiceProvider serviceProvider) : Job(serviceProvider)
{
    protected override async Task RunAsync(IJobExecutionContext context)
    {
        var manager = ResolveService<IMessageCacheManager>();
        context.Result = await manager.ProcessScheduledTaskAsync();
    }
}
