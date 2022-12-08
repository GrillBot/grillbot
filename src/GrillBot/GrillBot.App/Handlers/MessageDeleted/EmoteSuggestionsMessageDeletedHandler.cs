using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.MessageDeleted;

public class EmoteSuggestionsMessageDeletedHandler : IMessageDeleted
{
    private IMessageCacheManager MessageCache { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public EmoteSuggestionsMessageDeletedHandler(IMessageCacheManager messageCache, GrillBotDatabaseBuilder databaseBuilder)
    {
        MessageCache = messageCache;
        DatabaseBuilder = databaseBuilder;
    }

    public async Task ProcessAsync(Cacheable<IMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel)
    {
        if (!cachedChannel.HasValue || cachedChannel.Value is not IGuildChannel guildChannel) return;

        var message = cachedMessage.HasValue ? cachedMessage.Value : null;
        message ??= await MessageCache.GetAsync(cachedMessage.Id, null, true);
        if (message == null) return;

        await using var repository = DatabaseBuilder.CreateRepository();

        var suggestion = await repository.EmoteSuggestion.FindSuggestionByMessageId(guildChannel.GuildId, message.Id);
        if (suggestion == null || suggestion.VoteFinished || suggestion.VoteEndsAt != null) return; // Cannot delete processed suggestions.

        repository.Remove(suggestion);
        await repository.CommitAsync();
    }
}
