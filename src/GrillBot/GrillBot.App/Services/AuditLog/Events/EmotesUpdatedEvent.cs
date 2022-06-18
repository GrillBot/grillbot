using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog.Events;

public class EmotesUpdatedEvent : AuditEventBase
{
    private SocketGuild Before { get; }
    private SocketGuild After { get; }

    public EmotesUpdatedEvent(AuditLogService auditLogService, AuditLogWriter auditLogWriter, SocketGuild before, SocketGuild after) : base(auditLogService, auditLogWriter)
    {
        Before = before;
        After = after;
    }

    public override Task<bool> CanProcessAsync() =>
        Task.FromResult(!Before.Emotes.SequenceEqual(After.Emotes));

    public override async Task ProcessAsync()
    {
        var removedEmotes = Before.Emotes.Where(o => !After.Emotes.Contains(o)).ToList();
        if (removedEmotes.Count == 0) return;

        var auditLog = (await Before.GetAuditLogsAsync(DiscordConfig.MaxAuditLogEntriesPerBatch, actionType: ActionType.EmojiDeleted).FlattenAsync())
            .FirstOrDefault(o => removedEmotes.Any(x => x.Id == ((EmoteDeleteAuditLogData)o.Data).EmoteId));
        if (auditLog == null) return;

        var data = new AuditEmoteInfo((EmoteDeleteAuditLogData)auditLog.Data);

        var item = new AuditLogDataWrapper(AuditLogItemType.EmojiDeleted, data, Before, processedUser: auditLog.User, discordAuditLogItemId: auditLog.Id.ToString());
        await AuditLogWriter.StoreAsync(item);
    }
}
