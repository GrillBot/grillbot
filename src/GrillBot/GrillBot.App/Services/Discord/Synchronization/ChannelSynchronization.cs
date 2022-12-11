namespace GrillBot.App.Services.Discord.Synchronization;

public class ChannelSynchronization : SynchronizationBase
{
    public ChannelSynchronization(GrillBotDatabaseBuilder databaseBuilder) : base(databaseBuilder)
    {
    }

    public async Task ChannelDeletedAsync(ITextChannel channel)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var dbChannel = await repository.Channel.FindChannelByIdAsync(channel.Id, channel.Guild.Id);
        if (dbChannel == null) return;

        dbChannel.Update(channel);
        dbChannel.MarkDeleted(true);
        dbChannel.RolePermissionsCount = 0;
        dbChannel.UserPermissionsCount = 0;

        if (channel is not IThreadChannel)
        {
            var threads = await repository.Channel.GetChildChannelsAsync(channel.Id, channel.Guild.Id);
            threads.ForEach(o => o.MarkDeleted(true));
        }

        await repository.CommitAsync();
    }

    public async Task ThreadDeletedAsync(IThreadChannel threadChannel)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var thread = await repository.Channel.FindThreadAsync(threadChannel);
        if (thread == null) return;

        thread.Update(threadChannel);
        thread.MarkDeleted(true);
        await repository.CommitAsync();
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
