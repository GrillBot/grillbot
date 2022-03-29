using GrillBot.Data.Models;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog.Events;

public class OverwriteChangedEvent : AuditEventBase
{
    private SocketChannel Channel { get; }
    private DateTime NextEventAt { get; }

    private SocketGuild Guild => ((SocketGuildChannel)Channel).Guild;

    public OverwriteChangedEvent(AuditLogService auditLogService, SocketChannel channel, DateTime nextEventAt) : base(auditLogService)
    {
        Channel = channel;
        NextEventAt = nextEventAt;
    }

    public override Task<bool> CanProcessAsync()
    {
        return Task.FromResult(
            Channel is SocketGuildChannel &&
            DateTime.Now >= NextEventAt
        );
    }

    public override async Task ProcessAsync()
    {
        var timeLimit = DateTime.Now.AddDays(-7);
        var auditLogIds = await AuditLogService.GetDiscordAuditLogIdsAsync(Guild, Channel,
            new[] { AuditLogItemType.OverwriteCreated, AuditLogItemType.OverwriteDeleted, AuditLogItemType.OverwriteUpdated }, timeLimit);

        var guild = Guild;
        var auditLogs = new List<RestAuditLogEntry>();
        auditLogs.AddRange(await guild.GetAuditLogsAsync(100, actionType: ActionType.OverwriteCreated).FlattenAsync());
        auditLogs.AddRange(await guild.GetAuditLogsAsync(100, actionType: ActionType.OverwriteDeleted).FlattenAsync());
        auditLogs.AddRange(await guild.GetAuditLogsAsync(100, actionType: ActionType.OverwriteUpdated).FlattenAsync());
        auditLogs = auditLogs.FindAll(o => !auditLogIds.Contains(o.Id));

        if (auditLogs.Count == 0)
            return;

        var created = auditLogs.FindAll(o => o.Action == ActionType.OverwriteCreated && ((OverwriteCreateAuditLogData)o.Data).ChannelId == Channel.Id);
        var removed = auditLogs.FindAll(o => o.Action == ActionType.OverwriteDeleted && ((OverwriteDeleteAuditLogData)o.Data).ChannelId == Channel.Id);
        var updated = auditLogs.FindAll(o => o.Action == ActionType.OverwriteUpdated && ((OverwriteUpdateAuditLogData)o.Data).ChannelId == Channel.Id);

        var items = new List<AuditLogDataWrapper>();
        foreach (var log in created)
        {
            var data = new AuditOverwriteInfo(((OverwriteCreateAuditLogData)log.Data).Overwrite);
            items.Add(new AuditLogDataWrapper(AuditLogItemType.OverwriteCreated, data, guild, Channel, log.User, log.Id.ToString(), log.CreatedAt.LocalDateTime));
        }

        foreach (var log in removed)
        {
            var data = new AuditOverwriteInfo(((OverwriteDeleteAuditLogData)log.Data).Overwrite);
            items.Add(new AuditLogDataWrapper(AuditLogItemType.OverwriteDeleted, data, guild, Channel, log.User, log.Id.ToString(), log.CreatedAt.LocalDateTime));
        }

        foreach (var log in updated)
        {
            var auditData = (OverwriteUpdateAuditLogData)log.Data;
            var oldPerms = new Overwrite(auditData.OverwriteTargetId, auditData.OverwriteType, auditData.OldPermissions);
            var newPerms = new Overwrite(auditData.OverwriteTargetId, auditData.OverwriteType, auditData.NewPermissions);
            var data = new Diff<AuditOverwriteInfo>(new(oldPerms), new(newPerms));
            items.Add(new AuditLogDataWrapper(AuditLogItemType.OverwriteUpdated, data, guild, Channel, log.User, log.Id.ToString(), log.CreatedAt.LocalDateTime));
        }

        await AuditLogService.StoreItemsAsync(items);
    }
}
