using GrillBot.Core.RabbitMQ;
using GrillBot.Core.RabbitMQ.Publisher;
using System.Reflection;

namespace GrillBot.Common.Extensions.RabbitMQ;

public static class RabbitMQPublisherExtensions
{
    public static Task PushAsync<TPayload>(this IRabbitMQPublisher publisher, TPayload payload)
    {
        var queueName = payload is IPayload payloadData ? payloadData.QueueName : GetQueueName<TPayload>();
        if (string.IsNullOrEmpty(queueName))
            throw new ArgumentException("Unable to publish data to the queue without name.");

        return publisher.PublishAsync(queueName, payload);
    }

    private static string? GetQueueName<TPayload>()
    {
        return Array.Find(
            typeof(TPayload).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy),
            f => f.Name == "QueueName" && f.IsLiteral && !f.IsInitOnly
        )?.GetRawConstantValue() as string;
    }
}
