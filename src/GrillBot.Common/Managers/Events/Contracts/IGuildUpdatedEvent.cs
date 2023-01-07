using Discord;

namespace GrillBot.Common.Managers.Events.Contracts;

public interface IGuildUpdatedEvent
{
    Task ProcessAsync(IGuild before, IGuild after);
}
