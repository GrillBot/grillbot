using GrillBot.App.Helpers;
using GrillBot.App.Managers;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.ReactionRemoved;

public class PointsReactionRemovedHandler : IReactionRemovedEvent
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private PointsRecalculationManager PointsRecalculation { get; }

    public PointsReactionRemovedHandler(GrillBotDatabaseBuilder databaseBuilder, PointsRecalculationManager pointsRecalculation)
    {
        DatabaseBuilder = databaseBuilder;
        PointsRecalculation = pointsRecalculation;
    }

    public async Task ProcessAsync(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel, SocketReaction reaction)
    {
        if (!Init(cachedChannel, reaction, out var channel)) return;

        var user = reaction.User.IsSpecified ? reaction.User.Value as IGuildUser : null;
        user ??= await channel.Guild.GetUserAsync(reaction.UserId);

        await using var repository = DatabaseBuilder.CreateRepository();

        var reactionId = PointsHelper.CreateReactionId(reaction);
        var transaction = await repository.Points.FindTransactionAsync(channel.Guild, cachedMessage.Id, reactionId, user);
        if (transaction == null) return;

        repository.Remove(transaction);
        await repository.CommitAsync();

        var full = transaction.AssingnedAt.Date != DateTime.Now.Date;
        await PointsRecalculation.ComputeSummariesAsync(full, new List<IGuildUser> { user });
    }

    private static bool Init(Cacheable<IMessageChannel, ulong> cachedChannel, IReaction reaction, out IGuildChannel channel)
    {
        channel = cachedChannel.HasValue && cachedChannel.Value is IGuildChannel guildChannel ? guildChannel : null;

        if (channel == null) return false;
        return reaction.Emote is Emote && channel.Guild.Emotes.Any(x => x.IsEqual(reaction.Emote));
    }
}
