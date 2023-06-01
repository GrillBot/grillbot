using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.Synchronization.Database;

public class ChannelSynchronizationHandler : BaseSynchronizationHandler, IChannelCreatedEvent, IChannelDestroyedEvent, IChannelUpdatedEvent
{
    public ChannelSynchronizationHandler(GrillBotDatabaseBuilder databaseBuilder) : base(databaseBuilder)
    {
    }

    async Task IChannelCreatedEvent.ProcessAsync(IChannel channel)
    {
        if (channel is IThreadChannel || channel is not IGuildChannel guildChannel)
            return;

        await using var repository = CreateRepository();

        await repository.Guild.GetOrCreateGuildAsync(guildChannel.Guild);
        await repository.CommitAsync();

        await repository.Channel.GetOrCreateChannelAsync(guildChannel);
        await repository.CommitAsync();
    }

    async Task IChannelDestroyedEvent.ProcessAsync(IChannel channel)
    {
        if (channel is IThreadChannel || channel is not IGuildChannel guildChannel)
            return;

        await using var repository = CreateRepository();

        await repository.Guild.GetOrCreateGuildAsync(guildChannel.Guild);
        await repository.CommitAsync();

        var channelEntity = await repository.Channel.GetOrCreateChannelAsync(guildChannel);
        channelEntity.MarkDeleted(true);
        channelEntity.RolePermissionsCount = 0;
        channelEntity.UserPermissionsCount = 0;
        channelEntity.PinCount = 0;

        var threads = await repository.Channel.GetChildChannelsAsync(guildChannel.Id, guildChannel.GuildId);
        foreach (var thread in threads)
        {
            thread.MarkDeleted(true);
            thread.PinCount = 0;
        }

        await repository.CommitAsync();
    }

    public async Task ProcessAsync(IChannel before, IChannel after)
    {
        if (after is IThreadChannel || after is not IGuildChannel guildChannel)
            return;

        await using var repository = CreateRepository();

        await repository.Guild.GetOrCreateGuildAsync(guildChannel.Guild);
        await repository.CommitAsync();

        await repository.Channel.GetOrCreateChannelAsync(guildChannel);
        await repository.CommitAsync();
    }
}
