using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.RabbitMQ;
using GrillBot.Core.RabbitMQ.Publisher;

namespace GrillBot.App.Actions;

public class RabbitMQPublisherAction : ApiAction
{
    private IRabbitMQPublisher Publisher { get; }

    public RabbitMQPublisherAction(ApiRequestContext apiContext, IRabbitMQPublisher publisher) : base(apiContext)
    {
        Publisher = publisher;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var queueName = ((IPayload)Parameters[0]!).QueueName;
        var payload = Parameters[0]!;

        await Publisher.PublishAsync(queueName, payload, new());
        return ApiResult.Ok();
    }
}
