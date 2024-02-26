using AuditLogService.Models.Events.Create;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.RabbitMQ.Publisher;
using GrillBot.Core.Services.AuditLog.Enums;

namespace GrillBot.App.Actions.Api.V1.Invite;

public class RefreshMetadata : ApiAction
{
    private IDiscordClient DiscordClient { get; }
    private InviteManager InviteManager { get; }

    private readonly IRabbitMQPublisher _rabbitPublisher;

    public RefreshMetadata(ApiRequestContext apiContext, IDiscordClient discordClient, InviteManager inviteManager, IRabbitMQPublisher rabbitPublisher) : base(apiContext)
    {
        DiscordClient = discordClient;
        InviteManager = inviteManager;
        _rabbitPublisher = rabbitPublisher;
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

        var userId = ApiContext.GetUserId().ToString();
        var logRequest = new LogRequest(LogType.Info, DateTime.UtcNow, guild.Id.ToString(), userId)
        {
            LogMessage = new LogMessageRequest
            {
                Message = $"Invites for guild \"{guild.Name}\" was {(isReload ? "reloaded" : "loaded")}. Loaded invites: {invites.Count}",
                Severity = LogSeverity.Info,
                Source = $"Invite.{nameof(RefreshMetadata)}",
                SourceAppName = "GrillBot"
            }
        };

        await _rabbitPublisher.PublishAsync(new CreateItemsPayload(new() { logRequest }));
        await InviteManager.UpdateMetadataAsync(guild, invites);
        return invites.Count;
    }
}
