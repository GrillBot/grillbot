using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog.Events;

public class ChannelDeletedEvent : AuditEventBase
{
    private SocketChannel Channel { get; }
    private SocketGuildChannel GuildChannel => Channel as SocketGuildChannel;

    public ChannelDeletedEvent(AuditLogService auditLogService, SocketChannel channel) : base(auditLogService)
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

        var data = new AuditChannelInfo(auditLog.Data as ChannelDeleteAuditLogData, (channel as SocketTextChannel)?.Topic);
        var json = JsonConvert.SerializeObject(data, AuditLogService.JsonSerializerSettings);
        await AuditLogService.StoreItemAsync(AuditLogItemType.ChannelDeleted, channel.Guild, channel, auditLog.User, json, auditLog.Id, null, null);
    }
}
