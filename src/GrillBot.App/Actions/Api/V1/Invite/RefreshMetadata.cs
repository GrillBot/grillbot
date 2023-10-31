﻿using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.AuditLog;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Request.CreateItems;

namespace GrillBot.App.Actions.Api.V1.Invite;

public class RefreshMetadata : ApiAction
{
    private IDiscordClient DiscordClient { get; }
    private InviteManager InviteManager { get; }
    private IAuditLogServiceClient AuditLogServiceClient { get; }

    public RefreshMetadata(ApiRequestContext apiContext, IDiscordClient discordClient, InviteManager inviteManager, IAuditLogServiceClient auditLogServiceClient) : base(apiContext)
    {
        DiscordClient = discordClient;
        InviteManager = inviteManager;
        AuditLogServiceClient = auditLogServiceClient;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var isReload = (bool)Parameters[0]!;
        var result = await ProcessAsync(isReload);

        return ApiResult.Ok(result);
    }

    public async Task<Dictionary<string, int>> ProcessAsync(bool isReload)
    {
        var result = new Dictionary<string, int>();
        var guilds = await DiscordClient.GetGuildsAsync();

        foreach (var guild in guilds)
        {
            var count = await RefreshMetadataAsync(guild, isReload);
            result.Add(guild.Name, count);
        }

        return result;
    }

    private async Task<int> RefreshMetadataAsync(IGuild guild, bool isReload)
    {
        if (!await guild.CanManageInvitesAsync(DiscordClient.CurrentUser))
            return 0;

        var invites = await InviteManager.DownloadInvitesAsync(guild);
        if (invites.Count == 0)
            return 0;

        var logRequest = new LogRequest
        {
            Type = LogType.Info,
            CreatedAt = DateTime.UtcNow,
            GuildId = guild.Id.ToString(),
            LogMessage = new LogMessageRequest
            {
                Message = $"Invites for guild \"{guild.Name}\" was {(isReload ? "reloaded" : "loaded")}. Loaded invites: {invites.Count}",
                Severity = LogSeverity.Info,
                SourceAppName = "GrillBot",
                Source = $"Invite.{nameof(RefreshMetadata)}"
            },
            UserId = ApiContext.GetUserId().ToString()
        };
        await AuditLogServiceClient.CreateItemsAsync(new List<LogRequest> { logRequest });

        await InviteManager.UpdateMetadataAsync(guild, invites);
        return invites.Count;
    }
}
