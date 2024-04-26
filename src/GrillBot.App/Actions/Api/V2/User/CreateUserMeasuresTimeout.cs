using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.RabbitMQ.Publisher;
using GrillBot.Core.Services.UserMeasures.Models.Events;
using GrillBot.Data.Models.API.UserMeasures;
using Microsoft.AspNetCore.Http;

namespace GrillBot.App.Actions.Api.V2.User;

public class CreateUserMeasuresTimeout : ApiAction
{
    private readonly IRabbitMQPublisher _publisher;

    public CreateUserMeasuresTimeout(ApiRequestContext apiContext, IRabbitMQPublisher publisher) : base(apiContext)
    {
        _publisher = publisher;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var parameters = GetParameter<CreateUserMeasuresTimeoutParams>(0);
        var payload = new TimeoutPayload(
            parameters.CreatedAtUtc,
            parameters.Reason,
            parameters.GuildId,
            parameters.ModeratorId,
            parameters.TargetUserId,
            parameters.ValidToUtc,
            parameters.TimeoutId
        );

        await _publisher.PublishAsync(payload, new());
        return new ApiResult(StatusCodes.Status201Created);
    }
}
