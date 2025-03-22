using GrillBot.Core.RabbitMQ.V2.Publisher;
using GrillBot.Core.Services.UserMeasures.Models.Events;

namespace GrillBot.App.Actions.Commands.UserMeasures;

public class CreateUserMeasuresWarning : CommandAction
{
    private readonly IRabbitPublisher _rabbitPublisher;

    public CreateUserMeasuresWarning(IRabbitPublisher rabbitPublisher)
    {
        _rabbitPublisher = rabbitPublisher;
    }

    public Task ProcessAsync(IGuildUser user, string message, bool notification)
    {
        var moderatorId = Context.User.Id.ToString();
        var guildId = user.GuildId.ToString();

        var payload = new MemberWarningPayload(DateTime.UtcNow, message, guildId, moderatorId, user.Id.ToString(), notification);
        return _rabbitPublisher.PublishAsync(payload);
    }
}
