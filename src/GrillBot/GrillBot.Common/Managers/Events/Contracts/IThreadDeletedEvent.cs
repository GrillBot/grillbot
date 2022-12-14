using Discord;

namespace GrillBot.Common.Managers.Events.Contracts;

public interface IThreadDeletedEvent
{
    Task ProcessAsync(IThreadChannel? cachedThread, ulong threadId);
}
