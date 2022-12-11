namespace GrillBot.App.Services.Discord.Synchronization;

public class ChannelSynchronization : SynchronizationBase
{
    public ChannelSynchronization(GrillBotDatabaseBuilder databaseBuilder) : base(databaseBuilder)
    {
    }

    public async Task ThreadUpdatedAsync(IThreadChannel after)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var thread = await repository.Channel.FindThreadAsync(after);
        if (thread == null) return;

        thread.Update(after);
        await repository.CommitAsync();
    }
}
