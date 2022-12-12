using Discord;

namespace GrillBot.Common.Managers.Events.Contracts;

public interface IUserLeftEvent
{
    Task ProcessAsync(IGuild guild, IUser user);
}
