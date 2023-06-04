using GrillBot.App.Managers;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.Managers.Performance;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Handlers.ChannelDestroyed;

public class AuditChannelDestroyedHandler : IChannelDestroyedEvent
{
    private ICounterManager CounterManager { get; }
    private AuditLogWriteManager AuditLogWriteManager { get; }

    public AuditChannelDestroyedHandler(ICounterManager counterManager, AuditLogWriteManager auditLogWriteManager)
    {
        CounterManager = counterManager;
        AuditLogWriteManager = auditLogWriteManager;
    }

    public async Task ProcessAsync(IChannel channel)
    {
        if (channel is not IGuildChannel guildChannel) return;

        var auditLog = await FindAuditLogAsync(guildChannel);
        if (auditLog == null) return;

        var data = new AuditChannelInfo((ChannelDeleteAuditLogData)auditLog.Data, guildChannel);
        var item = new AuditLogDataWrapper(AuditLogItemType.ChannelDeleted, data, guildChannel.Guild, channel, auditLog.User, auditLog.Id.ToString());
        await AuditLogWriteManager.StoreAsync(item);
    }

    private async Task<IAuditLogEntry?> FindAuditLogAsync(IGuildChannel guildChannel)
    {
        IReadOnlyCollection<IAuditLogEntry> auditLogs;
        using (CounterManager.Create("Discord.API.AuditLog"))
        {
            auditLogs = await guildChannel.Guild.GetAuditLogsAsync(actionType: ActionType.ChannelDeleted);
        }

        return auditLogs
            .FirstOrDefault(o => ((ChannelDeleteAuditLogData)o.Data).ChannelId == guildChannel.Id);
    }
}
