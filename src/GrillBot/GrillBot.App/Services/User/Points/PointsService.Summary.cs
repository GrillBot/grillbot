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
        var reportData = new StringBuilder();
        
        if (users != null)
        {
            foreach (var user in users)
            {
                var transactions = await repository.Points.GetAllTransactionsAsync(onlyToday, user);
                var report = await RecalculatePointsSummaryAsync(repository, transactions);
                reportData.AppendLine(report);
            }
        }
        else
        {
            var transactions = await repository.Points.GetAllTransactionsAsync(false, null);
            var report = await RecalculatePointsSummaryAsync(repository, transactions);
            reportData.AppendLine(report);
        }

        await repository.CommitAsync();
        return reportData.ToString();
    }

    private static async Task<string> RecalculatePointsSummaryAsync(GrillBotRepository repository, IReadOnlyCollection<PointsTransaction> transactions)
    {
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
                MessagePoints = o.Where(x => !x.IsReaction).Sum(x => x.Points),
                ReactionPoints = o.Where(x => x.IsReaction).Sum(x => x.Points)
            })
            .ToList();

        // Check and set new summaries.
        foreach (var summary in newSummaries)
        {
            var oldSummary = summaries.Find(o => o.Equals(summary));
            if (oldSummary != null)
            {
                oldSummary.MessagePoints = summary.MessagePoints;
                oldSummary.ReactionPoints = summary.ReactionPoints;
            }
            else
            {
                await repository.AddAsync(summary);
            }
        }

        return summaries.Count == newSummaries.Count ? null : $"Summaries:{summaries.Count}, NewSummaries:{newSummaries.Count}";
    }
}
