using GrillBot.Data.Extensions.Discord;
using GrillBot.Data.Models;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog.Events;

public class ChannelUpdatedEvent : AuditEventBase
{
    private SocketChannel Before { get; }
    private SocketChannel After { get; }

    public ChannelUpdatedEvent(AuditLogService auditLogService, SocketChannel before, SocketChannel after) : base(auditLogService)
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
        var after = After as SocketGuildChannel;

        var auditLog = (await after.Guild.GetAuditLogsAsync(50, actionType: ActionType.ChannelUpdated).FlattenAsync())
            .FirstOrDefault(o => ((ChannelUpdateAuditLogData)o.Data).ChannelId == after.Id);
        if (auditLog == null) return;

        var auditData = auditLog.Data as ChannelUpdateAuditLogData;
        var data = new Diff<AuditChannelInfo>(new(Before.Id, auditData.Before), new(after.Id, auditData.After));
        var json = JsonConvert.SerializeObject(data, AuditLogService.JsonSerializerSettings);
        await AuditLogService.StoreItemAsync(AuditLogItemType.ChannelUpdated, after.Guild, after, auditLog.User, json, auditLog.Id, null, null);
    }
}
