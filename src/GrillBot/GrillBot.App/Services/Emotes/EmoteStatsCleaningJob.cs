using GrillBot.App.Services.AuditLog;
using GrillBot.App.Services.Discord;
using GrillBot.App.Services.Logging;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using Quartz;

namespace GrillBot.App.Services.Emotes;

[DisallowConcurrentExecution]
public class EmoteStatsCleaningJob : IJob
{
    private EmoteService EmoteService { get; }
    private LoggingService LoggingService { get; }
    private GrillBotContextFactory DbFactory { get; }
    private DiscordSocketClient DiscordClient { get; }
    private AuditLogService AuditLogService { get; }
    private DiscordInitializationService InitializationService { get; }

    public EmoteStatsCleaningJob(EmoteService emoteService, LoggingService loggingService, GrillBotContextFactory dbFactory,
        DiscordSocketClient discordClient, AuditLogService auditLogService, DiscordInitializationService initializationService)
    {
        EmoteService = emoteService;
        LoggingService = loggingService;
        DbFactory = dbFactory;
        DiscordClient = discordClient;
        AuditLogService = auditLogService;
        InitializationService = initializationService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await LoggingService.InfoAsync(nameof(EmoteStatsCleaningJob), $"Triggered {nameof(EmoteStatsCleaningJob)} at {DateTime.Now}");

        try
        {
            if (!InitializationService.Get()) return;
            using var dbContext = DbFactory.Create();

            var startAt = DateTime.Now;
            var emotesQuery = dbContext.Emotes.AsQueryable().Include(o => o.User);
            var emotes = await emotesQuery.ToListAsync(context.CancellationToken);
            var clearedRecords = 0;

            foreach (var emoteItem in emotes)
            {
                // Do not save changes in DB if job cannot succesfully complete.
                if (context.CancellationToken.IsCancellationRequested) return;

                // Ignore supported emotes.
                var emote = Emote.Parse(emoteItem.EmoteId);
                if (EmoteService.SupportedEmotes.Any(o => o.IsEqual(emote))) continue;

                dbContext.Remove(emoteItem);
                clearedRecords++;
            }

            if (clearedRecords > 0)
                await dbContext.SaveChangesAsync(context.CancellationToken);
            var report = $"{nameof(EmoteStatsCleaningJob)} completed (duration {DateTime.Now - startAt}, loaded records {emotes.Count}, cleared records {clearedRecords}).";
            var item = new AuditLogDataWrapper(AuditLogItemType.Info, report, processedUser: DiscordClient.CurrentUser);
            await AuditLogService.StoreItemAsync(item, context.CancellationToken);
        }
        catch (Exception ex)
        {
            await LoggingService.ErrorAsync(nameof(EmoteStatsCleaningJob), "An error occured at emote statistics cleaning.", ex);
        }
    }
}
