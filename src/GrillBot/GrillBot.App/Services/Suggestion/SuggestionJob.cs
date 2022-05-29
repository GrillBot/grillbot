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
    private SuggestionService SuggestionService { get; }
    private GrillBotDatabaseFactory DbFactory { get; }

    public SuggestionJob(LoggingService loggingService, AuditLogService auditLogService, IDiscordClient discordClient,
        InitManager initManager, SuggestionService suggestionService, GrillBotDatabaseFactory dbFactory)
        : base(loggingService, auditLogService, discordClient, initManager)
    {
        SuggestionService = suggestionService;
        DbFactory = dbFactory;
    }

    public override async Task RunAsync(IJobExecutionContext context)
    {
        await ProcessPendingSuggestions(context);
        SuggestionService.Sessions.PurgeExpired();
    }

    private async Task ProcessPendingSuggestions(IJobExecutionContext context)
    {
        using var dbContext = DbFactory.Create();
        var pendingSuggestions = await dbContext.Suggestions.OrderBy(o => o.Id).ToListAsync(context.CancellationToken);

        if (pendingSuggestions.Count == 0)
            return;

        var resultBuilder = new List<string>();

        foreach (var suggestion in pendingSuggestions)
        {
            var result = $"{suggestion.Id} ({suggestion.Type}/{suggestion.GuildId}/{suggestion.CreatedAt}) - ";

            try
            {
                await SuggestionService.ProcessPendingSuggestion(suggestion);

                dbContext.Remove(suggestion);
                result += "Success";
            }
            catch (Exception ex)
            {
                result += ex.Message;
                await LoggingService.ErrorAsync(JobName, ex.Message, ex);
            }

            resultBuilder.Add(result);
        }

        await dbContext.SaveChangesAsync();
        context.Result = string.Join("\n", resultBuilder);
    }
}
