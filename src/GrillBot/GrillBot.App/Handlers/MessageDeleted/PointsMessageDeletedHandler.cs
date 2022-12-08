using GrillBot.App.Managers;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Extensions;
using GrillBot.Common.Managers.Events.Contracts;

namespace GrillBot.App.Handlers.MessageDeleted;

public class PointsMessageDeletedHandler : IMessageDeleted
{
    private IMessageCacheManager MessageCache { get; }
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private PointsRecalculationManager PointsRecalculation { get; }

    public PointsMessageDeletedHandler(IMessageCacheManager messageCache, GrillBotDatabaseBuilder databaseBuilder, PointsRecalculationManager pointsRecalculation)
    {
        MessageCache = messageCache;
        DatabaseBuilder = databaseBuilder;
        PointsRecalculation = pointsRecalculation;
    }

    public async Task ProcessAsync(Cacheable<IMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel)
    {
        if (!cachedChannel.HasValue || cachedChannel.Value is not IGuildChannel channel) return;

        var message = cachedMessage.HasValue ? cachedMessage.Value : null;
        message ??= await MessageCache.GetAsync(cachedMessage.Id, null, true);
        if (message == null) return;

        await using var repository = DatabaseBuilder.CreateRepository();

        var transactions = await repository.Points.GetTransactionsAsync(message.Id, channel.Guild, null);
        if (transactions.Count == 0) return;

        repository.RemoveCollection(transactions);
        await repository.CommitAsync();

        var fullCompute = transactions.Any(o => o.AssingnedAt.Date != DateTime.Now.Date);
        var usersForUpdate = new List<IGuildUser>();
        foreach (var userId in transactions.Select(o => o.UserId.ToUlong()).Distinct())
            usersForUpdate.Add(await channel.GetUserAsync(userId));

        await PointsRecalculation.ComputeSummariesAsync(fullCompute, usersForUpdate);
    }
}
