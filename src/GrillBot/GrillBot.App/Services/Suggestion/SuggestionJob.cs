using GrillBot.App.Infrastructure.Jobs;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Logging;
using GrillBot.Common.Managers;
using Quartz;

namespace GrillBot.App.Services.Suggestion;

[DisallowConcurrentExecution]
[DisallowUninitialized]
public class SuggestionJob : Job
{
    private EmoteSuggestionService EmoteSuggestions { get; }
    private SuggestionSessionService SessionService { get; }

    public SuggestionJob(LoggingService loggingService, AuditLogWriter auditLogWriter, IDiscordClient discordClient,
        InitManager initManager, EmoteSuggestionService emoteSuggestionService, SuggestionSessionService sessionService)
        : base(loggingService, auditLogWriter, discordClient, initManager)
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
