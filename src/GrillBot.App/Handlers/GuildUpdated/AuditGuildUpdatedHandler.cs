using GrillBot.App.Helpers;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.Services.AuditLog;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Request.CreateItems;

namespace GrillBot.App.Handlers.GuildUpdated;

public class AuditGuildUpdatedHandler : AuditLogServiceHandler, IGuildUpdatedEvent
{
    private DownloadHelper DownloadHelper { get; }

    public AuditGuildUpdatedHandler(IAuditLogServiceClient client, DownloadHelper downloadHelper) : base(client)
    {
        DownloadHelper = downloadHelper;
    }

    public async Task ProcessAsync(IGuild before, IGuild after)
    {
        if (!CanProcess(before, after)) return;

        var infoBefore = await CreateInfoRequestAsync(before);
        var infoAfter = await CreateInfoRequestAsync(after);
        var request = CreateRequest(LogType.GuildUpdated, after);
        request.GuildUpdated = new DiffRequest<GuildInfoRequest>()
        {
            After = infoAfter,
            Before = infoBefore
        };

        await SendRequestAsync(request);
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
            IconData = await DownloadHelper.DownloadFileAsync(guild.IconUrl)
        };
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
