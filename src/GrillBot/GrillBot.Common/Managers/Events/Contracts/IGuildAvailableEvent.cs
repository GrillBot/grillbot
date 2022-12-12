using Discord;

namespace GrillBot.Common.Managers.Events.Contracts;

public interface IGuildAvailableEvent
{
    Task ProcessAsync(IGuild guild);
}
