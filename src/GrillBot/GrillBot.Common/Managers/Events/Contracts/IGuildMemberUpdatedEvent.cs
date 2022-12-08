using Discord;

namespace GrillBot.Common.Managers.Events.Contracts;

public interface IGuildMemberUpdatedEvent
{
    Task ProcessAsync(IGuildUser? before, IGuildUser after);
}
