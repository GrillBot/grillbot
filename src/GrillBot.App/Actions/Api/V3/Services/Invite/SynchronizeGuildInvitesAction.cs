using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.RabbitMQ.V2.Publisher;
using GrillBot.Core.Services.InviteService.Models.Events;

namespace GrillBot.App.Actions.Api.V3.Services.Invite;

public class SynchronizeGuildInvitesAction(
    ApiRequestContext context,
    IRabbitPublisher _rabbitPublisher
) : ApiAction(context)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var guildId = GetParameter<ulong>(0);
        var payload = new SynchronizeGuildInvitesPayload(guildId.ToString(), false);

        await _rabbitPublisher.PublishAsync(payload);
        await Task.Delay(1000);

        return ApiResult.Ok();
    }
}
