using GrillBot.App.Infrastructure;
using GrillBot.Cache.Services.Managers;

namespace GrillBot.App.Services.Suggestion;

[Initializable]
public class EmoteSuggestionsEventManager
{
    private DiscordSocketClient DiscordClient { get; }
    private EmoteSuggestionService EmoteSuggestionService { get; }
    private MessageCacheManager MessageCacheManager { get; }

    public EmoteSuggestionsEventManager(DiscordSocketClient discordClient, EmoteSuggestionService emoteSuggestionService,
        MessageCacheManager messageCacheManager)
    {
        DiscordClient = discordClient;
        EmoteSuggestionService = emoteSuggestionService;
        MessageCacheManager = messageCacheManager;

        DiscordClient.MessageDeleted += OnMessageDeletedAsync;
    }

    private async Task OnMessageDeletedAsync(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel)
    {
        if (!channel.HasValue) return;
        if (channel.Value is not IGuildChannel guildChannel) return;

        var msg = message.HasValue ? message.Value : await MessageCacheManager.GetAsync(message.Id, channel.Value, includeRemoved: true);
        if (msg is not IUserMessage userMessage) return;

        await EmoteSuggestionService.OnMessageDeletedAsync(userMessage, guildChannel.Guild);
    }
}
