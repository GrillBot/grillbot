using GrillBot.App.Helpers;
using GrillBot.App.Managers;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.ReactionAdded;

public class PointsReactionAddedHandler : IReactionAddedEvent
{
    private IMessageCacheManager MessageCache { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private PointsHelper PointsHelper { get; }
    private PointsRecalculationManager PointsRecalculation { get; }

    public PointsReactionAddedHandler(IMessageCacheManager messageCache, GrillBotDatabaseBuilder databaseBuilder, PointsHelper pointsHelper, PointsRecalculationManager pointsRecalculation)
    {
        MessageCache = messageCache;
        DatabaseBuilder = databaseBuilder;
        PointsHelper = pointsHelper;
        PointsRecalculation = pointsRecalculation;
    }

    public async Task ProcessAsync(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel, SocketReaction reaction)
    {
        if (!Init(cachedChannel, reaction, out var channel)) return;

        var user = (reaction.User.IsSpecified ? reaction.User.Value : await channel.Guild.GetUserAsync(reaction.UserId)) as IGuildUser;
        if (user == null || !user.IsUser()) return;

        var message = cachedMessage.HasValue ? cachedMessage.Value : null;
        message ??= await MessageCache.GetAsync(cachedMessage.Id, cachedChannel.Value) as IUserMessage;
        if (!PointsHelper.CanIncrementPoints(message) || message!.Author.Id == reaction.UserId) return;

        await using var repository = DatabaseBuilder.CreateRepository();

        await repository.Guild.GetOrCreateGuildAsync(channel.Guild);
        var userEntity = await repository.User.GetOrCreateUserAsync(user);
        var guildUserEntity = await repository.GuildUser.GetOrCreateGuildUserAsync(user);
        var guildChannel = await repository.Channel.FindChannelByIdAsync(channel.Id, channel.GuildId, true);
        if (!PointsHelper.CanIncrementPoints(userEntity, guildChannel)) return;

        var reactionId = PointsHelper.CreateReactionId(reaction);
        var transaction = PointsHelper.CreateTransaction(guildUserEntity, reactionId, message.Id, false);
        var migratedTransaction = PointsHelper.CreateMigratedTransaction(guildUserEntity, transaction);
        var transactions = await PointsHelper.FilterTransactionsAsync(repository, transaction, migratedTransaction);
        if (transactions.Count == 0) return;

        await repository.AddCollectionAsync(transactions);
        await repository.CommitAsync();
        await PointsRecalculation.ComputeSummariesAsync(false, new List<IGuildUser> { user });
    }

    private static bool Init(Cacheable<IMessageChannel, ulong> cachedChannel, IReaction reaction, out IGuildChannel channel)
    {
        channel = cachedChannel.HasValue && cachedChannel.Value is IGuildChannel guildChannel ? guildChannel : null;
        return channel != null && reaction.Emote is Emote && channel.Guild.Emotes.Any(o => o.IsEqual(reaction.Emote));
    }
}
