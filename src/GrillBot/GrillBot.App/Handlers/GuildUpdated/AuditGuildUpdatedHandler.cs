using GrillBot.App.Services.AuditLog;
using GrillBot.Common.Managers.Counters;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Handlers.GuildUpdated;

public class AuditGuildUpdatedHandler : IGuildUpdatedEvent
{
    private CounterManager CounterManager { get; }
    private AuditLogWriter AuditLogWriter { get; }

    public AuditGuildUpdatedHandler(CounterManager counterManager, AuditLogWriter auditLogWriter)
    {
        CounterManager = counterManager;
        AuditLogWriter = auditLogWriter;
    }

    public async Task ProcessAsync(IGuild before, IGuild after)
    {
        if (!CanProcess(before, after)) return;

        var auditLog = await FindAuditLogAsync(after);
        if (auditLog == null) return;

        var data = new GuildUpdatedData((SocketGuild)before, (SocketGuild)after);
        var item = new AuditLogDataWrapper(AuditLogItemType.GuildUpdated, data, after, processedUser: auditLog.User, discordAuditLogItemId: auditLog.Id.ToString());
        await AuditLogWriter.StoreAsync(item);
    }

    private static bool CanProcess(IGuild before, IGuild after)
    {
        return
            before.Name != after.Name ||
            before.AFKTimeout != after.AFKTimeout ||
            before.DefaultMessageNotifications != after.DefaultMessageNotifications ||
            before.MfaLevel != after.MfaLevel ||
            before.VerificationLevel != after.VerificationLevel ||
            before.ExplicitContentFilter != after.ExplicitContentFilter ||
            before.IconId != after.IconId ||
            before.SplashId != after.SplashId ||
            before.DiscoverySplashId != after.DiscoverySplashId ||
            before.AFKChannelId != after.AFKChannelId ||
            before.SystemChannelId != after.SystemChannelId ||
            before.RulesChannelId != after.RulesChannelId ||
            before.PublicUpdatesChannelId != after.PublicUpdatesChannelId ||
            before.OwnerId != after.OwnerId ||
            before.VoiceRegionId != after.VoiceRegionId ||
            before.Features.Value != after.Features.Value ||
            before.PremiumTier != after.PremiumTier ||
            before.BannerId != after.BannerId ||
            before.VanityURLCode != after.VanityURLCode ||
            before.SystemChannelFlags != after.SystemChannelFlags ||
            before.Description != after.Description ||
            before.NsfwLevel != after.NsfwLevel;
    }

    private async Task<IAuditLogEntry> FindAuditLogAsync(IGuild guild)
    {
        IReadOnlyCollection<IAuditLogEntry> auditLogs;
        using (CounterManager.Create("Discord.API.AuditLog"))
        {
            auditLogs = await guild.GetAuditLogsAsync(actionType: ActionType.GuildUpdated);
        }

        var timeLimit = DateTime.Now.AddMinutes(-5);
        return auditLogs
            .Where(o => o.CreatedAt.LocalDateTime >= timeLimit)
            .MaxBy(o => o.CreatedAt.LocalDateTime);
    }
}
