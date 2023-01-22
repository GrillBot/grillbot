using GrillBot.App.Infrastructure.Jobs;
using GrillBot.Cache.Services.Managers;
using Quartz;

namespace GrillBot.App.Jobs;

[DisallowConcurrentExecution]
[DisallowUninitialized]
public class SuggestionJob : Job
{
    private EmoteSuggestionManager CacheManager { get; }
    private Managers.EmoteSuggestion.EmoteSuggestionManager EmoteSuggestionManager { get; }

    public SuggestionJob(IServiceProvider serviceProvider, EmoteSuggestionManager cacheManager, Managers.EmoteSuggestion.EmoteSuggestionManager emoteSuggestionManager) : base(serviceProvider)
    {
        CacheManager = cacheManager;
        EmoteSuggestionManager = emoteSuggestionManager;
    }

    protected override async Task RunAsync(IJobExecutionContext context)
    {
        await CacheManager.PurgeExpiredAsync();
        context.Result = await EmoteSuggestionManager.ProcessJobAsync();
    }
}
