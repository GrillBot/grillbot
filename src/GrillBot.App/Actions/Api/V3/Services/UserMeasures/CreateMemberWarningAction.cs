using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.RabbitMQ.V2.Publisher;
using UserMeasures.Models.Events;
using GrillBot.Data.Models.API.UserMeasures;

namespace GrillBot.App.Actions.Api.V3.Services.UserMeasures;

public class CreateMemberWarningAction : ApiAction
{
    private readonly IRabbitPublisher _rabbitPublisher;

    public CreateMemberWarningAction(ApiRequestContext apiContext, IRabbitPublisher rabbitPublisher) : base(apiContext)
    {
        _rabbitPublisher = rabbitPublisher;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var request = GetParameter<CreateMemberWarningParams>(0);
        var moderatorId = ApiContext.GetUserId().ToString();
        var createdAt = DateTime.UtcNow;
        var payload = new MemberWarningPayload(createdAt, request.Message, request.GuildId, moderatorId, request.UserId, request.SendDmNotification);

        await _rabbitPublisher.PublishAsync(payload);
        return ApiResult.Ok();
    }
}
