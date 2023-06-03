using Discord;

namespace GrillBot.Common.Managers.Events.Contracts;

public interface IMessageUpdatedEvent
{
    Task ProcessAsync(Cacheable<IMessage, ulong> before, IMessage after, IMessageChannel channel);
}
