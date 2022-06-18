using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog.Events;

public class GuildUpdatedEvent : AuditEventBase
{
    private SocketGuild Before { get; }
    private SocketGuild After { get; }

    public GuildUpdatedEvent(AuditLogService auditLogService, AuditLogWriter auditLogWriter, SocketGuild before, SocketGuild after) : base(auditLogService, auditLogWriter)
    {
        Before = before;
        After = after;
    }

    public override Task<bool> CanProcessAsync()
    {
        return Task.FromResult(
            Before.DefaultMessageNotifications != After.DefaultMessageNotifications ||
            Before.Description != After.Description ||
            Before.VanityURLCode != After.VanityURLCode ||
            Before.BannerId != After.BannerId ||
            Before.DiscoverySplashId != After.DiscoverySplashId ||
            Before.SplashId != After.SplashId ||
            Before.IconId != After.IconId ||
            Before.VoiceRegionId != After.VoiceRegionId ||
            Before.OwnerId != After.OwnerId ||
            Before.PublicUpdatesChannel != After.PublicUpdatesChannel ||
            Before.RulesChannel?.Id != After.RulesChannel?.Id ||
            Before.SystemChannel?.Id != After.SystemChannel?.Id ||
            Before.AFKChannel?.Id != After.AFKChannel?.Id ||
            Before.AFKTimeout != After.AFKTimeout ||
            Before.Name != After.Name ||
            Before.MfaLevel != After.MfaLevel
        );
    }

    public override async Task ProcessAsync()
    {
        var timeLimit = DateTime.UtcNow.AddMinutes(-5);
        var auditLog = (await After.GetAuditLogsAsync(DiscordConfig.MaxAuditLogEntriesPerBatch, actionType: ActionType.GuildUpdated).FlattenAsync())
            .Where(o => o.CreatedAt.DateTime >= timeLimit)
            .MaxBy(o => o.CreatedAt.DateTime);
        if (auditLog == null) return;

        var data = new GuildUpdatedData(Before, After);
        var item = new AuditLogDataWrapper(AuditLogItemType.GuildUpdated, data, After, processedUser: auditLog.User, discordAuditLogItemId: auditLog.Id.ToString());
        await AuditLogWriter.StoreAsync(item);
    }
}
