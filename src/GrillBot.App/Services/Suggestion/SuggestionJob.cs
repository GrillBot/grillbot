using GrillBot.App.Infrastructure.Jobs;
using GrillBot.Cache.Services.Managers;
using Quartz;

namespace GrillBot.App.Services.Suggestion;

[DisallowConcurrentExecution]
[DisallowUninitialized]
public class SuggestionJob : Job
{
    private EmoteSuggestionService EmoteSuggestions { get; }
    private EmoteSuggestionManager CacheManager { get; }

    public SuggestionJob(EmoteSuggestionService emoteSuggestionService, IServiceProvider serviceProvider, EmoteSuggestionManager cacheManager) : base(serviceProvider)
    {
        EmoteSuggestions = emoteSuggestionService;
        CacheManager = cacheManager;
    }

    protected override async Task RunAsync(IJobExecutionContext context)
    {
        await CacheManager.PurgeExpiredAsync();

        context.Result = await EmoteSuggestions.ProcessJobAsync();
    }
}
