using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog.Events;

public class EmotesUpdatedEvent : AuditEventBase
{
    private SocketGuild Before { get; }
    private SocketGuild After { get; }

    public EmotesUpdatedEvent(AuditLogService auditLogService, SocketGuild before, SocketGuild after) : base(auditLogService)
    {
        Before = before;
        After = after;
    }

    public override Task<bool> CanProcessAsync() =>
        Task.FromResult(!Before.Emotes.SequenceEqual(After.Emotes));

    public override async Task ProcessAsync()
    {
        (List<GuildEmote> added, List<GuildEmote> removed) = new Func<(List<GuildEmote>, List<GuildEmote>)>(() =>
        {
            var removed = Before.Emotes.Where(e => !After.Emotes.Contains(e)).ToList();
            var added = After.Emotes.Where(e => !Before.Emotes.Contains(e)).ToList();
            return (added, removed);
        })();

        if (removed.Count == 0) return;

        var auditLog = (await Before.GetAuditLogsAsync(DiscordConfig.MaxAuditLogEntriesPerBatch, actionType: ActionType.EmojiDeleted).FlattenAsync())
            .FirstOrDefault(o => removed.Any(x => x.Id == ((EmoteDeleteAuditLogData)o.Data).EmoteId));

        var data = new AuditEmoteInfo(auditLog.Data as EmoteDeleteAuditLogData);

        var item = new AuditLogDataWrapper(AuditLogItemType.EmojiDeleted, data, Before, processedUser: auditLog.User, discordAuditLogItemId: auditLog.Id.ToString());
        await AuditLogService.StoreItemAsync(item);
    }
}
