using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.ChannelUpdated;

public class SyncChannelUpdatedHandler : IChannelUpdatedEvent
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public SyncChannelUpdatedHandler(GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task ProcessAsync(IChannel before, IChannel after)
    {
        if (after is IThreadChannel || after is not IGuildChannel guildChannel) return;

        await using var repository = DatabaseBuilder.CreateRepository();

        var channel = await repository.Channel.FindChannelByIdAsync(guildChannel.Id, guildChannel.GuildId);
        if (channel == null) return;

        channel.Update(guildChannel);
        await repository.CommitAsync();
    }
}
