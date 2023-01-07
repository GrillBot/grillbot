using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.ThreadUpdated;

public class SyncThreadUpdatedHandler : IThreadUpdatedEvent
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public SyncThreadUpdatedHandler(GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task ProcessAsync(IThreadChannel before, ulong threadId, IThreadChannel after)
    {
        if (!CanProcess(before, after)) return;

        await using var repository = DatabaseBuilder.CreateRepository();

        var thread = await repository.Channel.FindThreadAsync(after);
        if (thread == null) return;

        thread.Update(after);
        await repository.CommitAsync();
    }

    private static bool CanProcess(IThreadChannel before, IThreadChannel after)
        => before != null && (before.Name != after.Name || before.IsArchived != after.IsArchived);
}
