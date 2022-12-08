using Discord;

namespace GrillBot.Common.Managers.Events.Contracts;

public interface IMessageDeleted
{
    Task ProcessAsync(Cacheable<IMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel);
}
