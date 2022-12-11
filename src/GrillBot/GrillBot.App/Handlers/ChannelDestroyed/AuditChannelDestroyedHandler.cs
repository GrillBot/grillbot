﻿using GrillBot.App.Services.AuditLog;
using GrillBot.Common.Managers.Counters;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Handlers.ChannelDestroyed;

public class AuditChannelDestroyedHandler : IChannelDestroyedEvent
{
    private CounterManager CounterManager { get; }
    private AuditLogWriter AuditLogWriter { get; }

    public AuditChannelDestroyedHandler(CounterManager counterManager, AuditLogWriter auditLogWriter)
    {
        CounterManager = counterManager;
        AuditLogWriter = auditLogWriter;
    }

    public async Task ProcessAsync(IChannel channel)
    {
        if (channel is not IGuildChannel guildChannel) return;

        var auditLog = await FindAuditLogAsync(guildChannel);
        if (auditLog == null) return;

        var data = new AuditChannelInfo((ChannelDeleteAuditLogData)auditLog.Data, guildChannel);
        var item = new AuditLogDataWrapper(AuditLogItemType.ChannelDeleted, data, guildChannel.Guild, channel, auditLog.User, auditLog.Id.ToString());
        await AuditLogWriter.StoreAsync(item);
    }

    private async Task<IAuditLogEntry> FindAuditLogAsync(IGuildChannel guildChannel)
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
