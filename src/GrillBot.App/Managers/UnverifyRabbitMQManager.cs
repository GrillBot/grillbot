using GrillBot.Core.RabbitMQ.Publisher;
using GrillBot.Core.Services.UserMeasures.Models.Events;
using GrillBot.Data.Models.Unverify;
using GrillBot.Database.Entity;

namespace GrillBot.App.Managers;

public class UnverifyRabbitMQManager
{
    private IRabbitMQPublisher RabbitMQ { get; }

    public UnverifyRabbitMQManager(IRabbitMQPublisher rabbitMQ)
    {
        RabbitMQ = rabbitMQ;
    }

    public Task SendModifyAsync(long logSetId, DateTime newEnd)
    {
        var payload = new UnverifyModifyPayload(logSetId, newEnd);
        return RabbitMQ.PublishAsync(UnverifyModifyPayload.QueueName, payload);
    }

    public async Task SendUnverifyAsync(UnverifyUserProfile profile, UnverifyLog unverifyLog)
    {
        var payload = new UnverifyPayload(
            profile.Start.ToUniversalTime(),
            profile.Reason!,
            unverifyLog.GuildId,
            unverifyLog.FromUserId,
            unverifyLog.ToUserId,
            profile.End.ToUniversalTime(),
            unverifyLog.Id
        );

        await RabbitMQ.PublishAsync(UnverifyPayload.QueueName, payload);
    }
}
