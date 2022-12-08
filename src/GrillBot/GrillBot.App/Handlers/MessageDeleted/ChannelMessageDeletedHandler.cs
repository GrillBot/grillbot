using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.MessageDeleted;

public class ChannelMessageDeletedHandler : IMessageDeleted
{
    private IMessageCacheManager MessageCache { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IDiscordClient DiscordClient { get; }
    private IConfiguration Configuration { get; }

    public ChannelMessageDeletedHandler(IMessageCacheManager messageCache, GrillBotDatabaseBuilder databaseBuilder, IDiscordClient discordClient, IConfiguration configuration)
    {
        MessageCache = messageCache;
        DatabaseBuilder = databaseBuilder;
        DiscordClient = discordClient;
        Configuration = configuration;
    }

    public async Task ProcessAsync(Cacheable<IMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel)
    {
        if (!cachedChannel.HasValue || cachedChannel.Value is not ITextChannel textChannel) return;

        var message = cachedMessage.HasValue ? cachedMessage.Value : null;
        message ??= await MessageCache.GetAsync(cachedMessage.Id, null, true);
        if (message == null) return;

        var argPos = 0;
        var commandPrefix = Configuration.GetValue<string>("Discord:Commands:Prefix");
        if (message.IsCommand(ref argPos, DiscordClient.CurrentUser, commandPrefix)) return;

        await using var repository = DatabaseBuilder.CreateRepository();

        var statistics = await repository.Channel.FindUserChannelAsync(textChannel, message.Author);
        if (statistics == null || statistics.Count == 0) return;

        statistics.Count--;
        await repository.CommitAsync();
    }
}
