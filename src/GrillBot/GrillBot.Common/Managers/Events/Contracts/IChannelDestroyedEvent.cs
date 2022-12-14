using Discord;

namespace GrillBot.Common.Managers.Events.Contracts;

public interface IChannelDestroyedEvent
{
    Task ProcessAsync(IChannel channel);
}
