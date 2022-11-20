using GrillBot.Database.Entity;
using GrillBot.Database.Services.Repository;

namespace GrillBot.App.Services.User.Points;

public partial class PointsService
{
    public async Task<string> RecalculatePointsSummaryAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        return await RecalculatePointsSummaryAsync(repository, false);
    }

    private static async Task<string> RecalculatePointsSummaryAsync(GrillBotRepository repository, bool onlyToday, List<IGuildUser> users = null)
    {
        var startAt = DateTime.Now;
        var toProcess = new List<PointsTransaction>();

        if (users != null)
        {
            foreach (var user in users)
                toProcess.AddRange(await repository.Points.GetAllTransactionsAsync(onlyToday, user));
        }
        else
        {
            toProcess.AddRange(await repository.Points.GetAllTransactionsAsync(false, null));
        }

        var report = await RecalculatePointsSummaryAsync(repository, toProcess);
        if (report == null) return null; // Nothing to process.

        await repository.CommitAsync();
        return report.Replace("%DURATION%", (DateTime.Now - startAt).ToString("c"));
    }

    private static async Task<string> RecalculatePointsSummaryAsync(GrillBotRepository repository, IReadOnlyCollection<PointsTransaction> transactions)
    {
        if (transactions.Count == 0) return null; // Nothing to process.

        var dateFrom = transactions.Min(o => o.AssingnedAt).Date;
        var dateTo = transactions.Max(o => o.AssingnedAt).Date.Add(new TimeSpan(23, 59, 59));
        var guilds = transactions.Select(o => o.GuildId).Distinct().ToList();
        var users = transactions.Select(o => o.UserId).Distinct().ToList();

        var summaries = await repository.Points.GetSummariesAsync(dateFrom, dateTo, guilds, users);
        var newSummaries = transactions
            .GroupBy(o => new { o.GuildId, o.UserId, o.AssingnedAt.Date })
            .Select(o => new PointsTransactionSummary
            {
                GuildId = o.Key.GuildId,
                UserId = o.Key.UserId,
                Day = o.Key.Date,
                MessagePoints = o.Where(x => !x.IsReaction()).Sum(x => x.Points),
                ReactionPoints = o.Where(x => x.IsReaction()).Sum(x => x.Points)
            });

        // Check and set new summaries.
        var updatedCount = 0;
        var newItems = new List<PointsTransactionSummary>();
        foreach (var summary in newSummaries)
        {
            if (summaries.ContainsKey(summary.SummaryId))
            {
                var oldSummary = summaries[summary.SummaryId];
                var itemChanged = oldSummary.MessagePoints != summary.MessagePoints || oldSummary.ReactionPoints != summary.ReactionPoints;
                if (!itemChanged) continue;

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
}
