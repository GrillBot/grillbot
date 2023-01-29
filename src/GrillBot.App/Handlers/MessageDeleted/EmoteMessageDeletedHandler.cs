using GrillBot.App.Helpers;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.MessageDeleted;

public class EmoteMessageDeletedHandler : IMessageDeletedEvent
{
    private EmoteHelper EmoteHelper { get; }
    private IMessageCacheManager MessageCache { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private IDiscordClient DiscordClient { get; }

    public EmoteMessageDeletedHandler(EmoteHelper emoteHelper, IMessageCacheManager messageCache, GrillBotDatabaseBuilder databaseBuilder, IDiscordClient discordClient)
    {
        EmoteHelper = emoteHelper;
        MessageCache = messageCache;
        DatabaseBuilder = databaseBuilder;
        DiscordClient = discordClient;
    }

    public async Task ProcessAsync(Cacheable<IMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel)
    {
        if (!cachedChannel.HasValue || cachedChannel.Value is not ITextChannel) return;

        var supportedEmotes = await EmoteHelper.GetSupportedEmotesAsync();
        if (supportedEmotes.Count == 0) return;

        var message = cachedMessage.HasValue ? cachedMessage.Value : null;
        message ??= await MessageCache.GetAsync(cachedMessage.Id, null, true);
        if (message == null || message.IsCommand(DiscordClient.CurrentUser)) return;

        var emotes = message.GetEmotesFromMessage(supportedEmotes).ToList();
        if (emotes.Count == 0 || message.Author is not IGuildUser guildUser) return;

        await using var repository = DatabaseBuilder.CreateRepository();
        foreach (var emote in emotes)
        {
            var emoteEntity = await repository.Emote.FindStatisticAsync(emote, guildUser, guildUser.Guild);
            if (emoteEntity == null || emoteEntity.UseCount == 0) continue;

            emoteEntity.UseCount--;
        }

        await repository.CommitAsync();
    }
}
