using System.Xml.Linq;
using GrillBot.App.Services.AuditLog;
using GrillBot.Database.Entity;

namespace GrillBot.App.Services.User.Points;

public partial class PointsService
{
    public async Task<string> ArchiveOldTransactionsAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        if (!await repository.Points.ExistsExpiredItemsAsync())
            return null;

        var data = await repository.Points.GetExpiredTransactionsAsync();
        var logRoot = new XElement("Transactions");

        logRoot.Add(AuditClearingHelper.CreateMetadata(data.Count));
        logRoot.Add(AuditClearingHelper.TransformGuilds(data.Select(o => o.Guild)));
        logRoot.Add(AuditClearingHelper.TransformGuildUsers(data.Select(o => o.GuildUser)));

        foreach (var item in data)
        {
            var element = new XElement("Transaction");
            element.Add(
                new XAttribute("GuildId", item.GuildId),
                new XAttribute("UserId", item.UserId),
                new XAttribute("MessageId", item.MessageId),
                new XAttribute("ReactionId", item.ReactionId),
                new XAttribute("AssignedAt", item.AssingnedAt.ToString("o")),
                new XAttribute("Points", item.Points.ToString())
            );

            logRoot.Add(element);
            repository.Remove(item);
        }

        await ClearingHelper.StoreDataAsync(logRoot, Enumerable.Empty<AuditLogFileMeta>(), "PointsTransactions");
        await repository.CommitAsync();
        return $"Items: {data.Count}, XmlSize: {Encoding.UTF8.GetBytes(logRoot.ToString()).Length} B";
    }

    public async Task<string> ArchiveOldSummariesAsync()
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        if (!await repository.Points.ExistsExpiredSummariesAsync())
            return null;

        var data = await repository.Points.GetExpiredSummariesAsync();
        var logRoot = new XElement("Summaries");

        logRoot.Add(AuditClearingHelper.CreateMetadata(data.Count));
        logRoot.Add(AuditClearingHelper.TransformGuilds(data.Select(o => o.Guild)));
        logRoot.Add(AuditClearingHelper.TransformGuildUsers(data.Select(o => o.GuildUser)));

        foreach (var item in data)
        {
            var element = new XElement("Summary");
            element.Add(
                new XAttribute("GuildId", item.GuildId),
                new XAttribute("UserId", item.UserId),
                new XAttribute("Day", item.Day.ToString("o")),
                new XAttribute("MessagePoints", item.MessagePoints.ToString()),
                new XAttribute("ReactionPoints", item.ReactionPoints.ToString())
            );

            logRoot.Add(element);
            repository.Remove(item);
        }

        await ClearingHelper.StoreDataAsync(logRoot, Enumerable.Empty<AuditLogFileMeta>(), "PointsSummaries");
        await repository.CommitAsync();
        return $"Items: {data.Count}, XmlSize: {Encoding.UTF8.GetBytes(logRoot.ToString()).Length} B";
    }
}
