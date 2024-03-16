using System.Diagnostics.CodeAnalysis;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.ReactionAdded;

public class EmoteStatsReactionAddedHandler : IReactionAddedEvent
{
    private IMessageCacheManager MessageCache { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public EmoteStatsReactionAddedHandler(IMessageCacheManager messageCache, GrillBotDatabaseBuilder databaseBuilder)
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

        await using var repository = DatabaseBuilder.CreateRepository();

        await repository.Guild.GetOrCreateGuildAsync(textChannel.Guild);
        await repository.User.GetOrCreateUserAsync(author);
        await repository.User.GetOrCreateUserAsync(reactionUser);

        var reactionUserEntity = await repository.GuildUser.GetOrCreateGuildUserAsync(reactionUser);
        var authorUserEntity = await repository.GuildUser.GetOrCreateGuildUserAsync(author);

        reactionUserEntity.GivenReactions++;
        authorUserEntity.ObtainedReactions++;
        await repository.CommitAsync();
    }

    private static bool Init(Cacheable<IMessageChannel, ulong> cachedChannel, [MaybeNullWhen(false)] out ITextChannel textChannel)
    {
        textChannel = cachedChannel is { HasValue: true, Value: ITextChannel channel } ? channel : null;
        return textChannel != null;
    }
}
