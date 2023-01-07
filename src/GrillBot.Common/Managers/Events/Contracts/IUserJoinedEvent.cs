using Discord;

namespace GrillBot.Common.Managers.Events.Contracts;

public interface IUserJoinedEvent
{
    Task ProcessAsync(IGuildUser user);
}
