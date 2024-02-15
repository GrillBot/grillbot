using Discord;

namespace GrillBot.Common.Managers.Events.Contracts;

public interface IRoleDeletedEvent
{
    Task ProcessAsync(IRole role);
}
