using GrillBot.App.Infrastructure.Jobs;
using GrillBot.App.Services.AuditLog;
using GrillBot.Common.Managers;
using GrillBot.Common.Managers.Logging;
using Quartz;

namespace GrillBot.App.Services.Suggestion;

[DisallowConcurrentExecution]
[DisallowUninitialized]
public class SuggestionJob : Job
{
    private EmoteSuggestionService EmoteSuggestions { get; }
    private SuggestionSessionService SessionService { get; }

    public SuggestionJob(AuditLogWriter auditLogWriter, IDiscordClient discordClient, InitManager initManager, EmoteSuggestionService emoteSuggestionService, SuggestionSessionService sessionService,
        LoggingManager loggingManager) : base(auditLogWriter, discordClient, initManager, loggingManager)
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
