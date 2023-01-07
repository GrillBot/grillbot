using Discord;

namespace GrillBot.Common.Managers.Events.Contracts;

public interface IUserUnbannedEvent
{
    Task ProcessAsync(IUser user, IGuild guild);
}
