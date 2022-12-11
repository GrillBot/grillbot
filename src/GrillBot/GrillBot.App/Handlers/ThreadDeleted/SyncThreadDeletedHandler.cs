using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.ThreadDeleted;

public class SyncThreadDeletedHandler : IThreadDeletedEvent
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public SyncThreadDeletedHandler(GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task ProcessAsync(IThreadChannel cachedThread, ulong threadId)
    {
        if (cachedThread == null) return;

        await using var repository = DatabaseBuilder.CreateRepository();

        var thread = await repository.Channel.FindThreadAsync(cachedThread);
        if (thread == null) return;

        thread.Update(cachedThread);
        thread.MarkDeleted(true);
        await repository.CommitAsync();
    }
}
