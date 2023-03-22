using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.Managers.Discord;
using GrillBot.Database.Entity;
using GrillBot.Database.Services.Repository;

namespace GrillBot.App.Handlers.ReactionRemoved;

public class EmoteStatsReactionRemovedHandler : IReactionRemovedEvent
{
    private IEmoteManager EmoteManager { get; }
    private IMessageCacheManager MessageCache { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public EmoteStatsReactionRemovedHandler(IEmoteManager emoteManager, IMessageCacheManager messageCache, GrillBotDatabaseBuilder databaseBuilder)
    {
        EmoteManager = emoteManager;
        MessageCache = messageCache;
        DatabaseBuilder = databaseBuilder;
    }

    public async Task ProcessAsync(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel, SocketReaction reaction)
    {
        var supportedEmotes = await EmoteManager.GetSupportedEmotesAsync();
        if (!Init(cachedChannel, supportedEmotes, reaction, out var textChannel, out var emote)) return;

        var message = cachedMessage.HasValue ? cachedMessage.Value : null;
        message ??= await MessageCache.GetAsync(cachedMessage.Id, textChannel) as IUserMessage;
        if (message is not { Author: IGuildUser author } || author.Id == reaction.UserId) return;

        if ((reaction.User.IsSpecified ? reaction.User.Value : await textChannel.Guild.GetUserAsync(reaction.UserId)) is not IGuildUser reactionUser)
            return;

        await using var repository = DatabaseBuilder.CreateRepository();

        await UpdateEmoteStatsAsync(repository, reactionUser, emote, textChannel.Guild);
        var reactionUserEntity = await repository.GuildUser.FindGuildUserAsync(reactionUser);
        var authorUserEntity = await repository.GuildUser.FindGuildUserAsync(author);
        UpdateUserStats(authorUserEntity, reactionUserEntity);

        await repository.CommitAsync();
    }

    private static bool Init(Cacheable<IMessageChannel, ulong> cachedChannel, List<GuildEmote> supportedEmotes, IReaction reaction, out ITextChannel textChannel, out Emote emote)
    {
        textChannel = cachedChannel.HasValue && cachedChannel.Value is ITextChannel channel ? channel : null;
        emote = reaction.Emote is Emote tmpEmote && supportedEmotes.Count > 0 ? supportedEmotes.Find(o => o.IsEqual(tmpEmote)) : null;

        return textChannel != null && emote != null;
    }

    private static async Task UpdateEmoteStatsAsync(GrillBotRepository repository, IUser user, IEmote emote, IGuild guild)
    {
        var statistics = await repository.Emote.FindStatisticAsync(emote, user, guild);
        if (statistics == null || statistics.UseCount == 0) return;

        statistics.UseCount--;
    }

    private static void UpdateUserStats(GuildUser authorEntity, GuildUser reactingUserEntity)
    {
        if (reactingUserEntity is { GivenReactions: > 0 })
            reactingUserEntity.GivenReactions--;

        if (authorEntity is { ObtainedReactions: > 0 })
            authorEntity.ObtainedReactions--;
    }
}
