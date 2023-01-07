using Discord;

namespace GrillBot.Common.Managers.Events.Contracts;

public interface IThreadUpdatedEvent
{
    Task ProcessAsync(IThreadChannel? before, ulong threadId, IThreadChannel after);
}
