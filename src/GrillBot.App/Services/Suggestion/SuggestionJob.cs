using GrillBot.App.Infrastructure.Jobs;
using Quartz;

namespace GrillBot.App.Services.Suggestion;

[DisallowConcurrentExecution]
[DisallowUninitialized]
public class SuggestionJob : Job
{
    private EmoteSuggestionService EmoteSuggestions { get; }
    private SuggestionSessionService SessionService { get; }

    public SuggestionJob(EmoteSuggestionService emoteSuggestionService, SuggestionSessionService sessionService, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        EmoteSuggestions = emoteSuggestionService;
        SessionService = sessionService;
    }

    protected override async Task RunAsync(IJobExecutionContext context)
    {
        SessionService.PurgeExpired();
        context.Result = await EmoteSuggestions.ProcessJobAsync();
    }
}
