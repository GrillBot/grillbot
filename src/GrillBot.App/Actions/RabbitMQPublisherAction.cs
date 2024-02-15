using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
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
        var queueName = (string)Parameters[0]!;
        var payload = Parameters[1];

        await Publisher.PublishAsync(queueName, payload);
        return ApiResult.Ok();
    }
}
