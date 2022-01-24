using GrillBot.Data.Helpers;
using GrillBot.Data.Models;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog.Events;

public class OverwriteChangedEvent : AuditEventBase
{
    private SocketChannel Channel { get; }
    private DateTime NextEventAt { get; }
    private GrillBotContextFactory DbFactory { get; }

    private SocketGuild Guild => (Channel as SocketGuildChannel)?.Guild;

    public OverwriteChangedEvent(AuditLogService auditLogService, SocketChannel channel, DateTime nextEventAt,
        GrillBotContextFactory dbFactory) : base(auditLogService)
    {
        Channel = channel;
        NextEventAt = nextEventAt;
        DbFactory = dbFactory;
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
        var guild = Guild;

        using var context = DbFactory.Create();
        await context.InitGuildAsync(guild, CancellationToken.None);
        await context.InitGuildChannelAsync(guild, Channel, DiscordHelper.GetChannelType(Channel).Value, CancellationToken.None);

        var timeLimit = DateTime.Now.AddDays(-5);
        var auditLogIds = await AuditLogService.GetDiscordAuditLogIds(context, Guild, Channel,
            new[] { AuditLogItemType.OverwriteCreated, AuditLogItemType.OverwriteDeleted, AuditLogItemType.OverwriteUpdated }, timeLimit);

        var auditLogs = new List<RestAuditLogEntry>();
        auditLogs.AddRange(await guild.GetAuditLogsAsync(100, actionType: ActionType.OverwriteCreated).FlattenAsync());
        auditLogs.AddRange(await guild.GetAuditLogsAsync(100, actionType: ActionType.OverwriteDeleted).FlattenAsync());
        auditLogs.AddRange(await guild.GetAuditLogsAsync(100, actionType: ActionType.OverwriteUpdated).FlattenAsync());
        auditLogs = auditLogs.FindAll(o => !auditLogIds.Contains(o.Id));

        var created = auditLogs.FindAll(o => o.Action == ActionType.OverwriteCreated && ((OverwriteCreateAuditLogData)o.Data).ChannelId == Channel.Id);
        var removed = auditLogs.FindAll(o => o.Action == ActionType.OverwriteDeleted && ((OverwriteDeleteAuditLogData)o.Data).ChannelId == Channel.Id);
        var updated = auditLogs.FindAll(o => o.Action == ActionType.OverwriteUpdated && ((OverwriteUpdateAuditLogData)o.Data).ChannelId == Channel.Id);

        foreach (var log in created)
        {
            var data = new AuditOverwriteInfo(((OverwriteCreateAuditLogData)log.Data).Overwrite);
            var json = JsonConvert.SerializeObject(data, AuditLogService.JsonSerializerSettings);
            var entity = AuditLogItem.Create(AuditLogItemType.OverwriteCreated, guild, Channel, log.User, json, log.Id);

            await context.InitUserAsync(log.User, CancellationToken.None);
            await context.InitGuildUserAsync(guild, log.User as IGuildUser ?? guild.GetUser(log.User.Id), CancellationToken.None);
            await context.AddAsync(entity);
        }

        foreach (var log in removed)
        {
            var data = new AuditOverwriteInfo(((OverwriteDeleteAuditLogData)log.Data).Overwrite);
            var json = JsonConvert.SerializeObject(data, AuditLogService.JsonSerializerSettings);
            var entity = AuditLogItem.Create(AuditLogItemType.OverwriteDeleted, guild, Channel, log.User, json, log.Id);

            await context.InitUserAsync(log.User, CancellationToken.None);
            await context.InitGuildUserAsync(guild, log.User as IGuildUser ?? guild.GetUser(log.User.Id), CancellationToken.None);
            await context.AddAsync(entity);
        }

        foreach (var log in updated)
        {
            var auditData = (OverwriteUpdateAuditLogData)log.Data;
            var oldPerms = new Overwrite(auditData.OverwriteTargetId, auditData.OverwriteType, auditData.OldPermissions);
            var newPerms = new Overwrite(auditData.OverwriteTargetId, auditData.OverwriteType, auditData.NewPermissions);
            var data = new Diff<AuditOverwriteInfo>(new(oldPerms), new(newPerms));
            var json = JsonConvert.SerializeObject(data, AuditLogService.JsonSerializerSettings);
            var entity = AuditLogItem.Create(AuditLogItemType.OverwriteUpdated, guild, Channel, log.User, json, log.Id);

            await context.InitUserAsync(log.User, CancellationToken.None);
            await context.InitGuildUserAsync(guild, log.User as IGuildUser ?? guild.GetUser(log.User.Id), CancellationToken.None);
            await context.AddAsync(entity);
        }

        await context.SaveChangesAsync();
    }
}
