using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog.Events;

public class ChannelDeletedEvent : AuditEventBase
{
    private SocketChannel Channel { get; }
    private SocketGuildChannel GuildChannel => Channel as SocketGuildChannel;

    public ChannelDeletedEvent(AuditLogService auditLogService, AuditLogWriter auditLogWriter, SocketChannel channel) : base(auditLogService, auditLogWriter)
    {
        Channel = channel;
    }

    public override Task<bool> CanProcessAsync() =>
        Task.FromResult(GuildChannel != null);

    public override async Task ProcessAsync()
    {
        var channel = GuildChannel;
        var auditLog = (await channel.Guild.GetAuditLogsAsync(50, actionType: ActionType.ChannelDeleted).FlattenAsync())
            .FirstOrDefault(o => ((ChannelDeleteAuditLogData)o.Data).ChannelId == channel.Id);

        if (auditLog == null) return;

        var data = new AuditChannelInfo(auditLog.Data as ChannelDeleteAuditLogData, channel);
        var item = new AuditLogDataWrapper(AuditLogItemType.ChannelDeleted, data, channel.Guild, channel, auditLog.User, auditLog.Id.ToString());
        await AuditLogWriter.StoreAsync(item);
    }
}
