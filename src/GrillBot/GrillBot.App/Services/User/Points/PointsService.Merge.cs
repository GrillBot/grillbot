using GrillBot.Database.Entity;

namespace GrillBot.App.Services.User.Points;

public partial class PointsService
{
    public async Task<string> MergeOldTransactionsAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        if (!await repository.Points.ExistsExpiredItemsAsync())
            return null;

        var expiredTransactions = await repository.Points.GetExpiredTransactionsAsync();
        repository.RemoveCollection(expiredTransactions);

        var mergedTransactions = MergeTransactions(expiredTransactions);
        await repository.AddCollectionAsync(mergedTransactions);
        await repository.CommitAsync();

        return $"MergeTransactions(Expired:{expiredTransactions.Count}, Merged:{mergedTransactions.Count})";
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
                    AssingnedAt = DateTime.Now,
                    UserId = transaction.UserId,
                    MessageId = SnowflakeUtils.ToSnowflake(DateTimeOffset.Now).ToString()
                };

                result.Add(mergedItem);
            }

            mergedItem.Points += transaction.Points;

            if (transaction.AssingnedAt < (mergedItem.MergeRangeFrom ?? DateTime.MaxValue))
                mergedItem.MergeRangeFrom = transaction.AssingnedAt;
            if (transaction.AssingnedAt > (mergedItem.MergeRangeTo ?? DateTime.MinValue))
                mergedItem.MergeRangeTo = transaction.AssingnedAt;
            mergedItem.MergedItemsCount++;
        }

        return result;
    }

    public async Task<string> MergeSummariesAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();
        if (!await repository.Points.ExistsExpiredSummariesAsync())
            return null;

        var expiredItems = await repository.Points.GetExpiredSummariesAsync();
        repository.RemoveCollection(expiredItems);

        var mergedSummaries = MergeSummariesAsync(expiredItems);
        await repository.AddCollectionAsync(mergedSummaries);
        await repository.CommitAsync();

        return $"MergeSummaries(Expired:{expiredItems.Count}, Merged:{mergedSummaries.Count})";
    }

    private static List<PointsTransactionSummary> MergeSummariesAsync(List<PointsTransactionSummary> summaries)
    {
        var result = new List<PointsTransactionSummary>();

        foreach (var summary in summaries)
        {
            var mergedItem = result.Find(o => o.GuildId == summary.GuildId && o.UserId == summary.UserId);

            if (mergedItem == null)
            {
                mergedItem = new PointsTransactionSummary
                {
                    Day = DateTime.Now.Date,
                    GuildId = summary.GuildId,
                    UserId = summary.UserId,
                    IsMerged = true
                };

                result.Add(mergedItem);
            }

            mergedItem.MessagePoints += summary.MessagePoints;
            mergedItem.ReactionPoints += summary.ReactionPoints;

            if (summary.Day < (mergedItem.MergeRangeFrom ?? DateTime.MaxValue.Date))
                mergedItem.MergeRangeFrom = summary.Day;
            if (summary.Day > (mergedItem.MergeRangeTo ?? DateTime.MinValue.Date))
                mergedItem.MergeRangeTo = summary.Day;
            mergedItem.MergedItemsCount++;
        }

        return result;
    }
}
