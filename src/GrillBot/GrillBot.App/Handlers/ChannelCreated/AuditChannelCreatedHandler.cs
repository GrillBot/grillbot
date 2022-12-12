﻿using GrillBot.App.Services.AuditLog;
using GrillBot.Common.Managers.Counters;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Handlers.ChannelCreated;

public class AuditChannelCreatedHandler : IChannelCreatedEvent
{
    private CounterManager CounterManager { get; }
    private AuditLogWriter AuditLogWriter { get; }

    public AuditChannelCreatedHandler(CounterManager counterManager, AuditLogWriter auditLogWriter)
    {
        CounterManager = counterManager;
        AuditLogWriter = auditLogWriter;
    }

    public async Task ProcessAsync(IChannel channel)
    {
        if (channel is not IGuildChannel guildChannel) return;

        var auditLog = await FindAuditLogAsync(guildChannel);
        if (auditLog == null) return;

        var data = new AuditChannelInfo((ChannelCreateAuditLogData)auditLog.Data, guildChannel);
        var item = new AuditLogDataWrapper(AuditLogItemType.ChannelCreated, data, guildChannel.Guild, channel, auditLog.User, auditLog.Id.ToString());

        await AuditLogWriter.StoreAsync(item);
    }

    private async Task<IAuditLogEntry> FindAuditLogAsync(IGuildChannel channel)
    {
        IReadOnlyCollection<IAuditLogEntry> auditLogs;
        using (CounterManager.Create("Discord.API.AuditLog"))
        {
            auditLogs = await channel.Guild.GetAuditLogsAsync(actionType: ActionType.ChannelCreated);
        }

        return auditLogs
            .FirstOrDefault(o => ((ChannelCreateAuditLogData)o.Data).ChannelId == channel.Id);
    }
}
