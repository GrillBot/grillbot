using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.RabbitMQ.V2.Publisher;
using UserManagementService.Models.Events;

namespace GrillBot.App.Handlers.ServiceOrchestration;

public class UserManagementOrchestrationHandler(
    IRabbitPublisher _rabbitPublisher
) : IGuildMemberUpdatedEvent
{
    // GuildMemberUpdated
    public Task ProcessAsync(IGuildUser? before, IGuildUser after)
    {
        return before is null || before.Nickname == after.Nickname
            ? Task.CompletedTask
            : _rabbitPublisher.PublishAsync(NicknameChangedMessage.Create(before, after));
    }
}
