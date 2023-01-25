using GrillBot.Cache.Services.Managers.MessageCache;

namespace GrillBot.App.Managers.EmoteSuggestion;

public partial class EmoteSuggestionManager
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IDiscordClient DiscordClient { get; }
    private IMessageCacheManager MessageCacheManager { get; }

    public EmoteSuggestionManager(GrillBotDatabaseBuilder databaseBuilder, IDiscordClient discordClient, IMessageCacheManager messageCacheManager)
    {
        DatabaseBuilder = databaseBuilder;
        DiscordClient = discordClient;
        MessageCacheManager = messageCacheManager;
    }
}
