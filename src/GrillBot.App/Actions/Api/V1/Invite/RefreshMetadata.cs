using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.RabbitMQ.V2.Publisher;
using GrillBot.Core.Services.InviteService.Models.Events;

namespace GrillBot.App.Actions.Api.V1.Invite;

public class RefreshMetadata(
    ApiRequestContext apiContext,
    IDiscordClient _discordClient,
    IRabbitPublisher _rabbitPublisher
) : ApiAction(apiContext)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var guilds = await _discordClient.GetGuildsAsync();

        var payloads = guilds
            .Select(guild => new SynchronizeGuildInvitesPayload(guild.Id.ToString(), false))
            .ToList();

        await _rabbitPublisher.PublishAsync(payloads);
        return ApiResult.Ok();
    }
}
