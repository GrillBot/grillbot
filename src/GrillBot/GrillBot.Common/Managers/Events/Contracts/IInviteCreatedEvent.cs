using Discord;

namespace GrillBot.Common.Managers.Events.Contracts;

public interface IInviteCreatedEvent
{
    Task ProcessAsync(IInviteMetadata invite);
}
