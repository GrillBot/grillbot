using GrillBot.App.Infrastructure.Jobs;
using GrillBot.Cache.Services.Managers.MessageCache;
using Quartz;

namespace GrillBot.App.Services;

[DisallowConcurrentExecution]
[DisallowUninitialized]
public class MessageCacheJob : Job
{
    private IMessageCacheManager MessageCacheManager { get; }

    public MessageCacheJob(IServiceProvider serviceProvider, IMessageCacheManager messageCacheManager) : base(serviceProvider)
    {
        MessageCacheManager = messageCacheManager;
    }

    protected override async Task RunAsync(IJobExecutionContext context)
    {
        context.Result = await MessageCacheManager.ProcessScheduledTaskAsync();
    }
}
