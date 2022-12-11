using GrillBot.App.Managers;
using GrillBot.App.Services.AuditLog;
using GrillBot.Common.Managers.Counters;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Data.Models;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Handlers.ChannelUpdated;

public class AuditOverwritesChangedHandler : IChannelUpdatedEvent
{
    private AuditLogManager AuditLogManager { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private CounterManager CounterManager { get; }
    private AuditLogWriter AuditLogWriter { get; }

    public AuditOverwritesChangedHandler(AuditLogManager auditLogManager, GrillBotDatabaseBuilder databaseBuilder, CounterManager counterManager, AuditLogWriter auditLogWriter)
    {
        AuditLogManager = auditLogManager;
        DatabaseBuilder = databaseBuilder;
        CounterManager = counterManager;
        AuditLogWriter = auditLogWriter;
    }

    public async Task ProcessAsync(IChannel before, IChannel after)
    {
        if (!Init(after, out var guildChannel)) return;

        AuditLogManager.OnOverwriteChangedEvent(after.Id, DateTime.Now.AddMinutes(1));
        var ignoredLogIds = await GetIgnoredAuditLogIdsAsync(guildChannel);
        var auditLogs = await GetAuditLogsAsync(ignoredLogIds, guildChannel);
        if (auditLogs.Count == 0) return;

        var logItems = TransformItems(auditLogs, guildChannel).ToList();
        await AuditLogWriter.StoreAsync(logItems);
    }

    private bool Init(IChannel channel, out IGuildChannel guildChannelAfter)
    {
        guildChannelAfter = channel as IGuildChannel;

        if (guildChannelAfter == null) return false;
        return AuditLogManager.GetNextOverwriteEvent(channel.Id) <= DateTime.Now;
    }

    private async Task<List<ulong>> GetIgnoredAuditLogIdsAsync(IGuildChannel channel)
    {
        var timeLimit = DateTime.Now.AddMonths(-2);

        await using var repository = DatabaseBuilder.CreateRepository();
        return await repository.AuditLog.GetDiscordAuditLogIdsAsync(channel.Guild, channel,
            new[] { AuditLogItemType.OverwriteCreated, AuditLogItemType.OverwriteDeleted, AuditLogItemType.OverwriteUpdated }, timeLimit);
    }

    private async Task<List<IAuditLogEntry>> GetAuditLogsAsync(ICollection<ulong> ignoredLogIds, IGuildChannel channel)
    {
        var auditLogs = new List<IAuditLogEntry>();
        using (CounterManager.Create("Discord.API.AuditLogs"))
        {
            auditLogs.AddRange(await channel.Guild.GetAuditLogsAsync(actionType: ActionType.OverwriteCreated));
            auditLogs.AddRange(await channel.Guild.GetAuditLogsAsync(actionType: ActionType.OverwriteDeleted));
            auditLogs.AddRange(await channel.Guild.GetAuditLogsAsync(actionType: ActionType.OverwriteUpdated));
        }

        return auditLogs
            .FindAll(o => IsValidEntry(o, ignoredLogIds, channel));
    }

    private static bool IsValidEntry(IAuditLogEntry entry, ICollection<ulong> ignoredLogIds, IGuildChannel channel)
    {
        if (ignoredLogIds.Contains(entry.Id)) return false;

        return entry.Action switch
        {
            ActionType.OverwriteCreated => ((OverwriteCreateAuditLogData)entry.Data).ChannelId == channel.Id,
            ActionType.OverwriteDeleted => ((OverwriteDeleteAuditLogData)entry.Data).ChannelId == channel.Id,
            ActionType.OverwriteUpdated => ((OverwriteUpdateAuditLogData)entry.Data).ChannelId == channel.Id,
            _ => false
        };
    }

    private static IEnumerable<AuditLogDataWrapper> TransformItems(IEnumerable<IAuditLogEntry> auditLogs, IGuildChannel channel)
    {
        return auditLogs.Select(item => item.Action switch
        {
            ActionType.OverwriteCreated => CreateOverwriteCreatedData(item, channel),
            ActionType.OverwriteDeleted => CreateOverwriteDeletedData(item, channel),
            ActionType.OverwriteUpdated => CreateOverwriteUpdatedData(item, channel),
            _ => null
        }).Where(o => o != null);
    }

    private static AuditLogDataWrapper CreateOverwriteCreatedData(IAuditLogEntry entry, IGuildChannel channel)
    {
        var data = new AuditOverwriteInfo(((OverwriteCreateAuditLogData)entry.Data).Overwrite);
        return new AuditLogDataWrapper(AuditLogItemType.OverwriteCreated, data, channel.Guild, channel, entry.User, entry.Id.ToString(), entry.CreatedAt.LocalDateTime);
    }

    private static AuditLogDataWrapper CreateOverwriteDeletedData(IAuditLogEntry entry, IGuildChannel channel)
    {
        var data = new AuditOverwriteInfo(((OverwriteDeleteAuditLogData)entry.Data).Overwrite);
        return new AuditLogDataWrapper(AuditLogItemType.OverwriteDeleted, data, channel.Guild, channel, entry.User, entry.Id.ToString(), entry.CreatedAt.LocalDateTime);
    }

    private static AuditLogDataWrapper CreateOverwriteUpdatedData(IAuditLogEntry entry, IGuildChannel channel)
    {
        var auditData = (OverwriteUpdateAuditLogData)entry.Data;
        var oldPerms = new Overwrite(auditData.OverwriteTargetId, auditData.OverwriteType, auditData.OldPermissions);
        var newPerms = new Overwrite(auditData.OverwriteTargetId, auditData.OverwriteType, auditData.NewPermissions);
        var data = new Diff<AuditOverwriteInfo>(new AuditOverwriteInfo(oldPerms), new AuditOverwriteInfo(newPerms));
        return new AuditLogDataWrapper(AuditLogItemType.OverwriteUpdated, data, channel.Guild, channel, entry.User, entry.Id.ToString(), entry.CreatedAt.LocalDateTime);
    }
}
