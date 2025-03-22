using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.Synchronization.Database;

public class ThreadSynchronizationHandler : BaseSynchronizationHandler, IThreadDeletedEvent, IThreadUpdatedEvent
{
    public ThreadSynchronizationHandler(GrillBotDatabaseBuilder databaseBuilder) : base(databaseBuilder)
    {
    }

    public async Task ProcessAsync(IThreadChannel? cachedThread, ulong threadId)
    {
        if (cachedThread is null) return;

        using var repository = CreateRepository();

        await repository.Guild.GetOrCreateGuildAsync(cachedThread.Guild);
        await repository.CommitAsync();

        var thread = await repository.Channel.GetOrCreateChannelAsync(cachedThread);
        thread.MarkDeleted(true);
        thread.PinCount = 0;

        await repository.CommitAsync();
    }

    public async Task ProcessAsync(IThreadChannel? before, ulong threadId, IThreadChannel after)
    {
        if (before is null || (before.Name == after.Name && before.IsArchived == after.IsArchived))
            return;

        using var repository = CreateRepository();

        await repository.Guild.GetOrCreateGuildAsync(after.Guild);
        await repository.CommitAsync();

        await repository.Channel.GetOrCreateChannelAsync(after);
        await repository.CommitAsync();
    }
}
