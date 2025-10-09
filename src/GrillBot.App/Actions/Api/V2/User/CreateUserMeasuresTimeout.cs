using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.RabbitMQ.V2.Publisher;
using UserMeasures.Models.Events;
using GrillBot.Data.Models.API.UserMeasures;
using Microsoft.AspNetCore.Http;

namespace GrillBot.App.Actions.Api.V2.User;

public class CreateUserMeasuresTimeout : ApiAction
{
    private readonly IRabbitPublisher _publisher;

    public CreateUserMeasuresTimeout(ApiRequestContext apiContext, IRabbitPublisher publisher) : base(apiContext)
    {
        _publisher = publisher;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var parameters = GetParameter<CreateUserMeasuresTimeoutParams>(0);

        var payload = new TimeoutPayload(
            parameters.CreatedAtUtc.ToUniversalTime(),
            parameters.Reason,
            parameters.GuildId,
            parameters.ModeratorId,
            parameters.TargetUserId,
            parameters.ValidToUtc.ToUniversalTime(),
            parameters.TimeoutId
        );

        await _publisher.PublishAsync(payload);
        return new ApiResult(StatusCodes.Status201Created);
    }
}
