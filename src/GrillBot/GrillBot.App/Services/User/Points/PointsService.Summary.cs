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
        return report;
    }

    private static async Task<string> RecalculatePointsSummaryAsync(GrillBotRepository repository, IReadOnlyCollection<PointsTransaction> transactions)
    {
        if (transactions.Count == 0) return null; // Nothing to process.

        var dateFrom = transactions.Count == 0 ? DateTime.Now.AddYears(-1) : transactions.Min(o => o.AssingnedAt).AddDays(-1);
        var dateTo = transactions.Count == 0 ? DateTime.Now.AddYears(1) : transactions.Max(o => o.AssingnedAt).AddDays(1);
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
            })
            .ToList();

        // Check and set new summaries.
        var updated = 0;
        var inserted = 0;
        foreach (var summary in newSummaries)
        {
            var oldSummary = summaries.Find(o => o.Equals(summary));
            if (oldSummary != null)
            {
                var itemChanged = oldSummary.MessagePoints != summary.MessagePoints || oldSummary.ReactionPoints != summary.ReactionPoints;
                if (itemChanged) updated++;

                oldSummary.MessagePoints = summary.MessagePoints;
                oldSummary.ReactionPoints = summary.ReactionPoints;
            }
            else
            {
                await repository.AddAsync(summary);
                inserted++;
            }
        }

        if (inserted == 0 && updated == 0) return null; // Nothing to report, nothing was changed.

        var daysCount = Math.Round((dateTo - dateFrom).TotalDays);
        return $"RecalculatePoints(Days:{daysCount}, Created:{inserted}, Updated:{updated})";
    }
}
