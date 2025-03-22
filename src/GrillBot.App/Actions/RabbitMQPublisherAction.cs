using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.RabbitMQ.V2.Messages;
using GrillBot.Core.RabbitMQ.V2.Publisher;

namespace GrillBot.App.Actions;

public class RabbitMQPublisherAction : ApiAction
{
    private IRabbitPublisher Publisher { get; }

    public RabbitMQPublisherAction(ApiRequestContext apiContext, IRabbitPublisher publisher) : base(apiContext)
    {
        Publisher = publisher;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var queueName = ((IRabbitMessage)Parameters[0]!).Queue;
        var topicName = ((IRabbitMessage)Parameters[0]!).Topic;
        var payload = Parameters[0]!;

        await Publisher.PublishAsync(topicName, queueName, payload);
        return ApiResult.Ok();
    }
}
