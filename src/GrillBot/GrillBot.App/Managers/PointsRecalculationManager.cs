using GrillBot.Database.Entity;
using GrillBot.Database.Services.Repository;

namespace GrillBot.App.Managers;

/// <summary>
/// Points recalculation from transactions to summaries.
/// </summary>
public class PointsRecalculationManager
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }

    public PointsRecalculationManager(GrillBotDatabaseBuilder databaseBuilder)
    {
        DatabaseBuilder = databaseBuilder;
    }

    public async Task<string> ComputeSummariesAsync(bool full, List<IGuildUser> users)
    {
        var startAt = DateTime.Now;
        users ??= new List<IGuildUser>();

        await using var repository = DatabaseBuilder.CreateRepository();

        var transactions = await GetTransactionsAsync(repository, full, users);
        var report = await ProcessRecalculationAsync(repository, transactions);

        if (report == null) return null;

        await repository.CommitAsync();
        return report.Replace("%DURATION%", (DateTime.Now - startAt).ToString("c"));
    }

    private static async Task<List<PointsTransaction>> GetTransactionsAsync(GrillBotRepository repository, bool full, List<IGuildUser> users)
    {
        var transactions = new List<PointsTransaction>();

        if (users.Count == 0)
        {
            transactions.AddRange(await repository.Points.GetAllTransactionsAsync(full, null));
        }
        else
        {
            foreach (var user in users)
                transactions.AddRange(await repository.Points.GetAllTransactionsAsync(full, user));
        }

        return transactions;
    }

    private async Task<string> ProcessRecalculationAsync(GrillBotRepository repository, List<PointsTransaction> transactions)
    {
        if (transactions.Count == 0) return null; // Nothing to process.

        var (dateFrom, dateTo, guilds, users) = GetSummaryMetadata(transactions);
        var summaries = repository.Points.GetSummaries(dateFrom, dateTo, guilds, users);
        var newSummaries = transactions
            .GroupBy(o => new { o.GuildId, o.UserId, o.AssingnedAt.Date })
            .Select(o => ComputeSummary((o.Key.GuildId, o.Key.UserId, o.Key.Date), o));

        // Check and set new summaries.
        var updatedCount = 0;
        var newItems = new List<PointsTransactionSummary>();
        foreach (var summary in newSummaries)
        {
            if (summaries.ContainsKey(summary.SummaryId))
            {
                var oldSummary = summaries[summary.SummaryId];
                if (oldSummary.TotalPoints == summary.TotalPoints) continue;

                oldSummary.MessagePoints = summary.MessagePoints;
                oldSummary.ReactionPoints = summary.ReactionPoints;
                updatedCount++;
            }
            else
            {
                newItems.Add(summary);
            }
        }

        if (newItems.Count > 0)
        {
            await repository.AddCollectionAsync(newItems);
        }
        else
        {
            if (updatedCount == 0) return null; // Nothing to report, nothing was changed.
        }

        var daysCount = Math.Round((dateTo - dateFrom).TotalDays);
        var parts = new[]
        {
            $"Days:{daysCount}",
            newItems.Count > 0 ? $"Created:{newItems.Count}" : "",
            updatedCount > 0 ? $"Updated:{updatedCount}" : "",
            "%DURATION%",
            $"Transactions:{transactions.Count}",
            $"DateFrom:{dateFrom:o}, DateTo:{dateTo:o}",
            $"Guilds:{guilds.Count}, Users:{users.Count}"
        }.Where(o => !string.IsNullOrEmpty(o));

        return $"RecalculatePoints({string.Join(", ", parts)})";
    }

    private static (DateTime dateFrom, DateTime dateTo, HashSet<string> guilds, HashSet<string> users) GetSummaryMetadata(IEnumerable<PointsTransaction> transactions)
    {
        var dateFrom = DateTime.MaxValue;
        var dateTo = DateTime.MinValue;
        var guilds = new HashSet<string>();
        var users = new HashSet<string>();

        foreach (var transaction in transactions)
        {
            if (transaction.AssingnedAt.Date < dateFrom) dateFrom = transaction.AssingnedAt.Date;
            if (transaction.AssingnedAt.Date > dateTo) dateTo = transaction.AssingnedAt.Date;

            guilds.Add(transaction.GuildId);
            users.Add(transaction.UserId);
        }

        return (dateFrom, dateTo.Date.Add(new TimeSpan(23, 59, 59)), guilds, users);
    }

    private static PointsTransactionSummary ComputeSummary((string GuildId, string UserId, DateTime Date) key, IEnumerable<PointsTransaction> transactions)
    {
        var summary = new PointsTransactionSummary
        {
            GuildId = key.GuildId,
            UserId = key.UserId,
            Day = key.Date,
            MessagePoints = 0,
            ReactionPoints = 0
        };

        foreach (var item in transactions)
        {
            if (item.IsReaction())
                summary.ReactionPoints += item.Points;
            else
                summary.MessagePoints += item.Points;
        }

        return summary;
    }
}
