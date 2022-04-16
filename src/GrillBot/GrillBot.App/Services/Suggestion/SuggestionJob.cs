using GrillBot.App.Infrastructure.Jobs;
using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Discord;
using GrillBot.App.Services.Logging;
using Quartz;

namespace GrillBot.App.Services.Suggestion;

[DisallowConcurrentExecution]
[DisallowUninitialized]
public class SuggestionJob : Job
{
    private SuggestionService SuggestionService { get; }
    private GrillBotContextFactory DbFactory { get; }

    public SuggestionJob(LoggingService loggingService, AuditLogService auditLogService, IDiscordClient discordClient,
        DiscordInitializationService initializationService, SuggestionService suggestionService, GrillBotContextFactory dbFactory)
        : base(loggingService, auditLogService, discordClient, initializationService)
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
