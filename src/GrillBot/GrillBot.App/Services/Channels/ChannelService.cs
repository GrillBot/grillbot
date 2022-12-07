using GrillBot.App.Infrastructure;
using GrillBot.Cache.Services.Managers.MessageCache;

namespace GrillBot.App.Services.Channels;

[Initializable]
public class ChannelService
{
    private IMessageCacheManager MessageCache { get; }
    private DiscordSocketClient DiscordClient { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public ChannelService(DiscordSocketClient client, GrillBotDatabaseBuilder databaseBuilder,
        IMessageCacheManager messageCache)
    {
        MessageCache = messageCache;
        DiscordClient = client;
        DatabaseBuilder = databaseBuilder;

        DiscordClient.MessageDeleted += OnMessageRemovedAsync;
    }

    private async Task OnMessageRemovedAsync(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> messageChannel)
    {
        var msg = message.HasValue ? message.Value : await MessageCache.GetAsync(message.Id, null, true);
        if (!messageChannel.HasValue || msg == null || messageChannel.Value is not ITextChannel channel) return;

        await using var repository = DatabaseBuilder.CreateRepository();

        var channelEntity = await repository.Channel.FindChannelByIdAsync(channel.Id, channel.GuildId, true);
        var userChannel = channelEntity?.Users.FirstOrDefault(o => o.UserId == msg.Author.Id.ToString());
        if (userChannel == null) return;

        userChannel.Count--;
        await repository.CommitAsync();
    }
}
