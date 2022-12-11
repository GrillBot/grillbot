using GrillBot.App.Services.AuditLog;
using GrillBot.Common.Managers.Counters;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Handlers.GuildUpdated;

public class AuditEmotesGuildUpdatedHandler : IGuildUpdatedEvent
{
    private CounterManager CounterManager { get; }
    private AuditLogWriter AuditLogWriter { get; }

    public AuditEmotesGuildUpdatedHandler(CounterManager counterManager, AuditLogWriter auditLogWriter)
    {
        CounterManager = counterManager;
        AuditLogWriter = auditLogWriter;
    }

    public async Task ProcessAsync(IGuild before, IGuild after)
    {
        if (!CanProcess(before, after)) return;

        // TODO Implement sticker support.
        var logItems = await GetDeletedEmotesAsync(before, after);
        await AuditLogWriter.StoreAsync(logItems);
    }

    private static bool CanProcess(IGuild before, IGuild after)
    {
        return !before.Emotes.Select(o => o.Id).SequenceEqual(after.Emotes.Select(o => o.Id));
    }

    private async Task<List<AuditLogDataWrapper>> GetDeletedEmotesAsync(IGuild before, IGuild after)
    {
        var removedEmotes = before.Emotes.Where(o => !after.Emotes.Contains(o)).ToList();
        if (removedEmotes.Count == 0) return new List<AuditLogDataWrapper>();

        IReadOnlyCollection<IAuditLogEntry> auditLogs;
        using (CounterManager.Create("Discord.API.AuditLog"))
        {
            auditLogs = await after.GetAuditLogsAsync(actionType: ActionType.EmojiDeleted);
        }

        return removedEmotes
            .Select(e => auditLogs.FirstOrDefault(o => ((EmoteDeleteAuditLogData)o.Data).EmoteId == e.Id))
            .Where(o => o != null)
            .Select(logItem =>
            {
                var data = new AuditEmoteInfo((EmoteDeleteAuditLogData)logItem.Data);
                return new AuditLogDataWrapper(AuditLogItemType.EmojiDeleted, data, after, processedUser: logItem.User, discordAuditLogItemId: logItem.Id.ToString());
            }).ToList();
    }
}
