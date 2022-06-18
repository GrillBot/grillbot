using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog.Events;

public class ChannelCreatedEvent : AuditEventBase
{
    private SocketChannel Channel { get; }
    private SocketGuildChannel GuildChannel => Channel as SocketGuildChannel;

    public ChannelCreatedEvent(AuditLogService auditLogService, AuditLogWriter auditLogWriter, SocketChannel channel) : base(auditLogService, auditLogWriter)
    {
        Channel = channel;
    }

    public override Task<bool> CanProcessAsync() =>
        Task.FromResult(GuildChannel != null);

    public override async Task ProcessAsync()
    {
        var channel = GuildChannel;
        var auditLog = (await channel.Guild.GetAuditLogsAsync(DiscordConfig.MaxAuditLogEntriesPerBatch, actionType: ActionType.ChannelCreated).FlattenAsync())
            .FirstOrDefault(o => ((ChannelCreateAuditLogData)o.Data).ChannelId == channel.Id);

        if (auditLog == null) return;

        var data = new AuditChannelInfo(auditLog.Data as ChannelCreateAuditLogData);
        var item = new AuditLogDataWrapper(AuditLogItemType.ChannelCreated, data, channel.Guild, channel, auditLog.User, auditLog.Id.ToString());
        await AuditLogWriter.StoreAsync(item);
    }
}
