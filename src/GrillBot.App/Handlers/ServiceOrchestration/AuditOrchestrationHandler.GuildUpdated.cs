using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Events.Create;

namespace GrillBot.App.Handlers.ServiceOrchestration;

public partial class AuditOrchestrationHandler
{
    private static void ProcessRemovedEmotes(IGuild before, IGuild after, CreateItemsPayload payload)
    {
        var removedEmotes = before.Emotes.Where(e => !after.Emotes.Contains(e)).ToList();
        if (removedEmotes.Count == 0) return;

        var guildId = after.Id.ToString();

        foreach (var emote in removedEmotes)
        {
            payload.Items.Add(new LogRequest(LogType.EmoteDeleted, DateTime.UtcNow, guildId)
            {
                DeletedEmote = new DeletedEmoteRequest { EmoteId = emote.ToString() }
            });
        }
    }

    private async Task ProcessGuildChangesAsync(IGuild before, IGuild after, CreateItemsPayload payload)
    {
        if (!IsGuildChanged(before, after)) return;

        var infoBefore = await CreateInfoRequestAsync(before);
        var infoAfter = await CreateInfoRequestAsync(after);
        var guildId = after.Id.ToString();
        var logRequest = new LogRequest(LogType.GuildUpdated, DateTime.UtcNow, guildId)
        {
            GuildUpdated = new DiffRequest<GuildInfoRequest>
            {
                After = infoAfter,
                Before = infoBefore
            }
        };

        payload.Items.Add(logRequest);
    }

    private async Task<GuildInfoRequest> CreateInfoRequestAsync(IGuild guild)
    {
        return new GuildInfoRequest
        {
            Description = guild.Description,
            Features = guild.Features.Value,
            Name = guild.Name,
            BannerId = guild.BannerId,
            MfaLevel = guild.MfaLevel,
            PremiumTier = guild.PremiumTier,
            SplashId = guild.SplashId,
            NsfwLevel = guild.NsfwLevel,
            VerificationLevel = guild.VerificationLevel,
            IconId = guild.IconId,
            DefaultMessageNotifications = guild.DefaultMessageNotifications,
            DiscoverySplashId = guild.DiscoverySplashId,
            ExplicitContentFilter = guild.ExplicitContentFilter,
            RulesChannelId = guild.RulesChannelId?.ToString(),
            SystemChannelFlags = guild.SystemChannelFlags,
            PublicUpdatesChannelId = guild.PublicUpdatesChannelId?.ToString(),
            SystemChannelId = guild.SystemChannelId?.ToString(),
            AfkTimeout = guild.AFKTimeout,
            AfkChannelId = guild.AFKChannelId?.ToString(),
            VanityUrl = guild.VanityURLCode,
            IconData = await _downloadHelper.DownloadFileAsync(guild.IconUrl)
        };
    }

    private static bool IsGuildChanged(IGuild before, IGuild after)
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
            before.VoiceRegionId != after.VoiceRegionId ||
            before.Features.Value != after.Features.Value ||
            before.PremiumTier != after.PremiumTier ||
            before.BannerId != after.BannerId ||
            before.VanityURLCode != after.VanityURLCode ||
            before.SystemChannelFlags != after.SystemChannelFlags ||
            before.Description != after.Description ||
            before.NsfwLevel != after.NsfwLevel;
    }
}
