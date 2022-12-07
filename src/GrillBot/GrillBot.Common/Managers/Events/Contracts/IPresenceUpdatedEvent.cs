using Discord;

namespace GrillBot.Common.Managers.Events.Contracts;

public interface IPresenceUpdatedEvent
{
    Task ProcessAsync(IUser user, IPresence before, IPresence after);
}
