using GrillBot.App.Infrastructure.Jobs;
using GrillBot.Core.Services.Common.Executor;
using GrillBot.Core.Services.Emote;
using Quartz;

namespace GrillBot.App.Jobs;

[DisallowConcurrentExecution]
[DisallowUninitialized]
public class EmoteSuggestionsJob(
    IServiceProvider serviceProvider,
    IEmoteServiceClient _emoteService
) : Job(serviceProvider)
{
    protected override async Task RunAsync(IJobExecutionContext context)
    {
        var result = await _emoteService.FinishSuggestionVotesAsync();

        if (result > 0)
            context.Result = $"Finished votes: {result}";
    }
}
