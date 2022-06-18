using GrillBot.Common.Extensions.Discord;
using GrillBot.Data.Models;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog.Events;

public class ChannelUpdatedEvent : AuditEventBase
{
    private SocketChannel Before { get; }
    private SocketChannel After { get; }

    public ChannelUpdatedEvent(AuditLogService auditLogService, AuditLogWriter auditLogWriter, SocketChannel before, SocketChannel after) : base(auditLogService, auditLogWriter)
    {
        Before = before;
        After = after;
    }

    public override Task<bool> CanProcessAsync()
    {
        return Task.FromResult(
            Before is SocketGuildChannel before &&
            After is SocketGuildChannel after &&
            !before.IsEqual(after)
        );
    }

    public override async Task ProcessAsync()
    {
        var after = (SocketGuildChannel)After;

        var auditLog = (await after.Guild.GetAuditLogsAsync(DiscordConfig.MaxAuditLogEntriesPerBatch, actionType: ActionType.ChannelUpdated).FlattenAsync())
            .FirstOrDefault(o => ((ChannelUpdateAuditLogData)o.Data).ChannelId == after.Id);
        if (auditLog == null) return;

        var auditData = (ChannelUpdateAuditLogData)auditLog.Data;
        var data = new Diff<AuditChannelInfo>(new AuditChannelInfo(Before.Id, auditData.Before), new AuditChannelInfo(after.Id, auditData.After));
        var item = new AuditLogDataWrapper(AuditLogItemType.ChannelUpdated, data, after.Guild, after, auditLog.User, auditLog.Id.ToString());
        await AuditLogWriter.StoreAsync(item);
    }
}
