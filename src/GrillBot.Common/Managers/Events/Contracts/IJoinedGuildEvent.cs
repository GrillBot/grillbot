using Discord;

namespace GrillBot.Common.Managers.Events.Contracts;

public interface IJoinedGuildEvent
{
    Task ProcessAsync(IGuild guild);
}
