using GrillBot.Core.Extensions;
using GrillBot.Core.RabbitMQ.V2.Publisher;
using GrillBot.Core.Services.UserMeasures.Models.Events;
using GrillBot.Data.Models.Unverify;
using GrillBot.Database.Entity;

namespace GrillBot.App.Managers;

public class UnverifyRabbitMQManager
{
    private IRabbitPublisher RabbitMQ { get; }

    public UnverifyRabbitMQManager(IRabbitPublisher rabbitMQ)
    {
        RabbitMQ = rabbitMQ;
    }

    public Task SendModifyAsync(long logSetId, DateTime newEndUtc)
        => RabbitMQ.PublishAsync(new UnverifyModifyPayload(logSetId, newEndUtc));

    public async Task SendUnverifyAsync(UnverifyUserProfile profile, UnverifyLog unverifyLog)
    {
        var payload = new UnverifyPayload(
            profile.Start.WithKind(DateTimeKind.Local).ToUniversalTime(),
            profile.Reason!,
            unverifyLog.GuildId,
            unverifyLog.FromUserId,
            unverifyLog.ToUserId,
            profile.End.WithKind(DateTimeKind.Local).ToUniversalTime(),
            unverifyLog.Id
        );

        await RabbitMQ.PublishAsync(payload);
    }
}
