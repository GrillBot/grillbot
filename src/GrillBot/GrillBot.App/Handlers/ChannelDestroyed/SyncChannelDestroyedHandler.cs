using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.ChannelDestroyed;

public class SyncChannelDestroyedHandler : IChannelDestroyedEvent
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public SyncChannelDestroyedHandler(GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task ProcessAsync(IChannel channel)
    {
        if (channel is IThreadChannel || channel is not IGuildChannel guildChannel) return;

        await using var repository = DatabaseBuilder.CreateRepository();

        var channelEntity = await repository.Channel.FindChannelByIdAsync(guildChannel.Id, guildChannel.GuildId);
        if (channelEntity == null) return;

        channelEntity.Update(guildChannel);
        channelEntity.MarkDeleted(true);
        channelEntity.RolePermissionsCount = 0;
        channelEntity.UserPermissionsCount = 0;

        var threads = await repository.Channel.GetChildChannelsAsync(guildChannel.Id, guildChannel.GuildId);
        foreach (var thread in threads)
            thread.MarkDeleted(true);

        await repository.CommitAsync();
    }
}
