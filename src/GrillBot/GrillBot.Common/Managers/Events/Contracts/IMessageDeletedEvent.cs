using Discord;

namespace GrillBot.Common.Managers.Events.Contracts;

public interface IMessageDeletedEvent
{
    Task ProcessAsync(Cacheable<IMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel);
}
