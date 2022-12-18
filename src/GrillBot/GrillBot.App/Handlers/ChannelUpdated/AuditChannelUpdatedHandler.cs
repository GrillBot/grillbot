using GrillBot.App.Managers;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Counters;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Data.Models;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Handlers.ChannelUpdated;

public class AuditChannelUpdatedHandler : IChannelUpdatedEvent
{
    private CounterManager CounterManager { get; }
    private AuditLogWriteManager AuditLogWriteManager { get; }

    public AuditChannelUpdatedHandler(CounterManager counterManager, AuditLogWriteManager auditLogWriteManager)
    {
        CounterManager = counterManager;
        AuditLogWriteManager = auditLogWriteManager;
    }

    public async Task ProcessAsync(IChannel before, IChannel after)
    {
        if (!Init(before, after, out var guildChannelBefore, out var guildChannelAfter)) return;

        var auditLog = await FindAuditLogAsync(guildChannelAfter);
        if (auditLog == null && guildChannelBefore.Position == guildChannelAfter.Position) return;

        var auditData = (ChannelUpdateAuditLogData)auditLog?.Data;
        var data = CreateLogData(auditData, guildChannelBefore, guildChannelAfter);
        var item = new AuditLogDataWrapper(AuditLogItemType.ChannelUpdated, data, guildChannelAfter.Guild, guildChannelAfter, auditLog?.User, auditLog?.Id.ToString());
        await AuditLogWriteManager.StoreAsync(item);
    }

    private static bool Init(IChannel before, IChannel after, out IGuildChannel guildChannelBefore, out IGuildChannel guildChannelAfter)
    {
        guildChannelBefore = before as IGuildChannel;
        guildChannelAfter = after as IGuildChannel;

        if (guildChannelBefore == null || guildChannelAfter == null) return false;
        return !guildChannelBefore.IsEqual(guildChannelAfter);
    }

    private async Task<IAuditLogEntry> FindAuditLogAsync(IGuildChannel channel)
    {
        IReadOnlyCollection<IAuditLogEntry> auditLogs;
        using (CounterManager.Create("Discord.API.AuditLog"))
        {
            auditLogs = await channel.Guild.GetAuditLogsAsync(actionType: ActionType.ChannelUpdated);
        }

        return auditLogs
            .FirstOrDefault(o => ((ChannelUpdateAuditLogData)o.Data).ChannelId == channel.Id);
    }

    private static Diff<AuditChannelInfo> CreateLogData(ChannelUpdateAuditLogData auditData, IGuildChannel before, IGuildChannel after)
    {
        // Position change is not logged into discord audit log.
        var infoBefore = auditData == null ? new AuditChannelInfo(before) : new AuditChannelInfo(auditData.ChannelId, auditData.Before, before);
        var infoAfter = auditData == null ? new AuditChannelInfo(after) : new AuditChannelInfo(auditData.ChannelId, auditData.After, after);

        return new Diff<AuditChannelInfo>(infoBefore, infoAfter);
    }
}
