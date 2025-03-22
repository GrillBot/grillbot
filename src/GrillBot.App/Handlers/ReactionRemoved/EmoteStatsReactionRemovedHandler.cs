using System.Diagnostics.CodeAnalysis;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Database.Entity;

namespace GrillBot.App.Handlers.ReactionRemoved;

public class EmoteStatsReactionRemovedHandler : IReactionRemovedEvent
{
    private IMessageCacheManager MessageCache { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public EmoteStatsReactionRemovedHandler(IMessageCacheManager messageCache, GrillBotDatabaseBuilder databaseBuilder)
    {
        MessageCache = messageCache;
        DatabaseBuilder = databaseBuilder;
    }

    public async Task ProcessAsync(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel, SocketReaction reaction)
    {
        if (!Init(cachedChannel, out var textChannel)) return;

        var message = cachedMessage.HasValue ? cachedMessage.Value : null;
        message ??= await MessageCache.GetAsync(cachedMessage.Id, textChannel) as IUserMessage;
        if (message is not { Author: IGuildUser author } || author.Id == reaction.UserId) return;

        if ((reaction.User.IsSpecified ? reaction.User.Value : await textChannel.Guild.GetUserAsync(reaction.UserId)) is not IGuildUser reactionUser)
            return;

        using var repository = DatabaseBuilder.CreateRepository();

        var reactionUserEntity = await repository.GuildUser.FindGuildUserAsync(reactionUser);
        var authorUserEntity = await repository.GuildUser.FindGuildUserAsync(author);
        UpdateUserStats(authorUserEntity, reactionUserEntity);

        await repository.CommitAsync();
    }

    private static bool Init(Cacheable<IMessageChannel, ulong> cachedChannel, [MaybeNullWhen(false)] out ITextChannel textChannel)
    {
        textChannel = cachedChannel is { HasValue: true, Value: ITextChannel channel } ? channel : null;
        return textChannel is not null;
    }

    private static void UpdateUserStats(GuildUser? authorEntity, GuildUser? reactingUserEntity)
    {
        if (reactingUserEntity is { GivenReactions: > 0 })
            reactingUserEntity.GivenReactions--;

        if (authorEntity is { ObtainedReactions: > 0 })
            authorEntity.ObtainedReactions--;
    }
}
