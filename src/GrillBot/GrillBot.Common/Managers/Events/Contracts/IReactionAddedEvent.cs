using Discord;
using Discord.WebSocket;

namespace GrillBot.Common.Managers.Events.Contracts;

public interface IReactionAddedEvent
{
    Task ProcessAsync(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel, SocketReaction reaction);
}
