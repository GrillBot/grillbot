using GrillBot.App.Infrastructure.Jobs;
using GrillBot.Database.Entity;
using Quartz;

namespace GrillBot.App.Jobs;

[DisallowConcurrentExecution]
public class PointsJob : Job
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public PointsJob(GrillBotDatabaseBuilder databaseBuilder, IServiceProvider serviceProvider) : base(serviceProvider)
    {
        DatabaseBuilder = databaseBuilder;
    }

    protected override async Task RunAsync(IJobExecutionContext context)
    {
        var startAt = DateTime.Now;

        await using var repository = DatabaseBuilder.CreateRepository();
        if (!await repository.Points.ExistsExpiredItemsAsync()) return;

        var expiredTransactions = await repository.Points.GetExpiredTransactionsAsync();
        repository.RemoveCollection(expiredTransactions);

        var mergedTransactions = MergeTransactions(expiredTransactions);
        await repository.AddCollectionAsync(mergedTransactions);
        await repository.CommitAsync();

        context.Result = $"MergeTransactions(Expired:{expiredTransactions.Count}, Merged:{mergedTransactions.Count}, {DateTime.Now - startAt:c})";
    }

    private static List<PointsTransaction> MergeTransactions(List<PointsTransaction> transactions)
    {
        var result = new List<PointsTransaction>();

        foreach (var transaction in transactions)
        {
            var reactionId = $"Reactions_{transaction.GuildId}_{transaction.UserId}";
            var mergedItems = result.FindAll(o => o.GuildId == transaction.GuildId && o.UserId == transaction.UserId);
            var mergedItem = transaction.IsReaction() ? mergedItems.Find(o => o.ReactionId == reactionId) : mergedItems.Find(o => !o.IsReaction());

            if (mergedItem == null)
            {
                mergedItem = new PointsTransaction
                {
                    GuildId = transaction.GuildId,
                    ReactionId = transaction.IsReaction() ? reactionId : "",
                    UserId = transaction.UserId,
                    MessageId = SnowflakeUtils.ToSnowflake(DateTimeOffset.Now).ToString(),
                    AssingnedAt = DateTime.MaxValue
                };

                result.Add(mergedItem);
            }

            mergedItem.Points += transaction.Points;

            if (transaction.AssingnedAt <= (mergedItem.MergeRangeFrom ?? DateTime.MaxValue))
                mergedItem.MergeRangeFrom = transaction.AssingnedAt;
            if (transaction.AssingnedAt >= (mergedItem.MergeRangeTo ?? DateTime.MinValue))
                mergedItem.MergeRangeTo = transaction.AssingnedAt;
            mergedItem.AssingnedAt = mergedItem.MergeRangeFrom.GetValueOrDefault();
            mergedItem.MergedItemsCount++;
        }

        return result;
    }
}
