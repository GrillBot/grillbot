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
        await ProcessAsync((CreateUserMeasuresWarningParams)Parameters[0]!);
        return ApiResult.Ok();
    }

    public async Task ProcessAsync(IGuildUser user, string message)
    {
        var parameters = new CreateUserMeasuresWarningParams
        {
            GuildId = user.GuildId.ToString(),
            Message = message,
            UserId = user.Id.ToString()
        };

        await ProcessAsync(parameters);
    }

    private async Task ProcessAsync(CreateUserMeasuresWarningParams parameters)
    {
        var payload = new MemberWarningPayload
        {
            Reason = parameters.Message,
            CreatedAt = DateTime.UtcNow,
            GuildId = parameters.GuildId,
            ModeratorId = ApiContext.GetUserId().ToString(),
            TargetUserId = parameters.UserId
        };

        await RabbitMQ.PublishAsync(MemberWarningPayload.QueueName, payload);
    }
}
