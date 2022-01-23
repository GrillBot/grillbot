using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog.Events;

public class GuildUpdatedEvent : AuditEventBase
{
    private SocketGuild Before { get; }
    private SocketGuild After { get; }

    public GuildUpdatedEvent(AuditLogService auditLogService, SocketGuild before, SocketGuild after) : base(auditLogService)
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
            .OrderByDescending(o => o.CreatedAt.DateTime)
            .FirstOrDefault();

        var data = new GuildUpdatedData(Before, After);
        var json = JsonConvert.SerializeObject(data, AuditLogService.JsonSerializerSettings);
        await AuditLogService.StoreItemAsync(AuditLogItemType.GuildUpdated, After, null, auditLog.User, json, auditLog.Id, null, null);
    }
}
