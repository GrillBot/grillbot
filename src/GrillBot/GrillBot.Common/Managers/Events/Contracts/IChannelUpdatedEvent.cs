using Discord;

namespace GrillBot.Common.Managers.Events.Contracts;

public interface IChannelUpdatedEvent
{
    Task ProcessAsync(IChannel before, IChannel after);
}
