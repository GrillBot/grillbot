using Discord;

namespace GrillBot.Common.Managers.Events.Contracts;

public interface IUserUpdatedEvent
{
    Task ProcessAsync(IUser before, IUser after);
}
