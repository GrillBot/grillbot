using Discord;

namespace GrillBot.Common.Managers.Events.Contracts;

public interface IChannelCreatedEvent
{
    Task ProcessAsync(IChannel channel);
}
