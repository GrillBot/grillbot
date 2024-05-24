using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.RabbitMQ.Publisher;
using GrillBot.Core.Services.UserMeasures.Models.Events;
using GrillBot.Data.Models.API.UserMeasures;

namespace GrillBot.App.Actions.Api.V1.UserMeasures;

public class CreateUserMeasuresWarning : ApiAction
{
    private IRabbitMQPublisher RabbitMQ { get; }

    public CreateUserMeasuresWarning(ApiRequestContext apiContext, IRabbitMQPublisher rabbitMQ) : base(apiContext)
    {
        RabbitMQ = rabbitMQ;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        await ProcessAsync((CreateMemberWarningParams)Parameters[0]!);
        return ApiResult.Ok();
    }

    public async Task ProcessAsync(IGuildUser user, string message, bool notification)
    {
        var parameters = new CreateMemberWarningParams
        {
            GuildId = user.GuildId.ToString(),
            Message = message,
            UserId = user.Id.ToString(),
            SendDmNotification = notification
        };

        await ProcessAsync(parameters);
    }

    private async Task ProcessAsync(CreateMemberWarningParams parameters)
    {
        var moderatorId = ApiContext.GetUserId().ToString();
        var payload = new MemberWarningPayload(
            DateTime.UtcNow,
            parameters.Message,
            parameters.GuildId,
            moderatorId,
            parameters.UserId,
            parameters.SendDmNotification
        );

        await RabbitMQ.PublishAsync(payload, new());
    }
}
