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
        var payload = new UnverifyModifyPayload
        {
            LogSetId = logSetId,
            NewEnd = newEnd
        };

        return RabbitMQ.PublishAsync(UnverifyModifyPayload.QueueName, payload);
    }

    public async Task SendUnverifyAsync(UnverifyUserProfile profile, UnverifyLog unverifyLog)
    {
        var payload = new UnverifyPayload
        {
            CreatedAt = profile.Start.ToUniversalTime(),
            EndAt = profile.End.ToUniversalTime(),
            GuildId = unverifyLog.GuildId,
            LogSetId = unverifyLog.Id,
            ModeratorId = unverifyLog.FromUserId,
            Reason = profile.Reason!,
            TargetUserId = unverifyLog.ToUserId
        };

        await RabbitMQ.PublishAsync(UnverifyPayload.QueueName, payload);
    }
}
