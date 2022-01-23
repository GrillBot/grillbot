using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog.Events;

public class ChannelCreatedEvent : AuditEventBase
{
    private SocketChannel Channel { get; }
    private SocketGuildChannel GuildChannel => Channel as SocketGuildChannel;

    public ChannelCreatedEvent(AuditLogService auditLogService, SocketChannel channel) : base(auditLogService)
    {
        Channel = channel;
    }

    public override Task<bool> CanProcessAsync() =>
        Task.FromResult(GuildChannel != null);

    public override async Task ProcessAsync()
    {
        var channel = GuildChannel;
        var auditLog = (await channel.Guild.GetAuditLogsAsync(10, actionType: ActionType.ChannelCreated).FlattenAsync())
            .FirstOrDefault(o => ((ChannelCreateAuditLogData)o.Data).ChannelId == channel.Id);

        if (auditLog == null) return;

        var data = new AuditChannelInfo(auditLog.Data as ChannelCreateAuditLogData);
        var json = JsonConvert.SerializeObject(data, AuditLogService.JsonSerializerSettings);
        await AuditLogService.StoreItemAsync(AuditLogItemType.ChannelCreated, channel.Guild, channel, auditLog.User, json, auditLog.Id);
    }
}
