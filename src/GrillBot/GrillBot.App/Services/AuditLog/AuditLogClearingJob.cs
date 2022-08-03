using GrillBot.App.Infrastructure.Jobs;
using GrillBot.App.Services.Logging;
using GrillBot.Common.Managers;
using Quartz;
using System.Xml.Linq;
using GrillBot.Common.FileStorage;

namespace GrillBot.App.Services.AuditLog;

[DisallowConcurrentExecution]
public class AuditLogClearingJob : Job
{
    private GrillBotDatabaseBuilder DbFactory { get; }
    private AuditClearingHelper Helper { get; }

    public AuditLogClearingJob(LoggingService loggingService, AuditLogWriter auditLogWriter, IDiscordClient discordClient,
        GrillBotDatabaseBuilder dbFactory, InitManager initManager, AuditClearingHelper helper)
        : base(loggingService, auditLogWriter, discordClient, initManager)
    {
        DbFactory = dbFactory;
        Helper = helper;
    }

    protected override async Task RunAsync(IJobExecutionContext context)
    {
        var expirationDate = DateTime.Now.AddYears(-1);

        await using var repository = DbFactory.CreateRepository();

        if (!await repository.AuditLog.ExistsExpiredItemAsync(expirationDate))
            return;

        var data = await repository.AuditLog.GetExpiredDataAsync(expirationDate);
        var logRoot = new XElement("AuditLogBackup");

        logRoot.Add(AuditClearingHelper.CreateMetadata(data.Count));
        logRoot.Add(AuditClearingHelper.TransformGuilds(data.Select(o => o.Guild)));

        var guildUsers = data.Select(o => o.ProcessedGuildUser).Where(o => o != null).DistinctBy(o => $"{o.UserId}/{o.GuildId}").ToList();
        var users = data.Select(o => o.ProcessedUser).Where(o => o != null && guildUsers.All(x => x.UserId != o.Id)).DistinctBy(o => o.Id).ToList();

        logRoot.Add(AuditClearingHelper.TransformGuildUsers(guildUsers));
        logRoot.Add(AuditClearingHelper.TransformUsers(users));
        logRoot.Add(AuditClearingHelper.TransformChannels(data.Select(o => o.GuildChannel)));

        foreach (var item in data)
        {
            var element = new XElement("Item");

            element.Add(
                new XAttribute("Id", item.Id),
                new XAttribute("Type", item.Type.ToString()),
                new XAttribute("CreatedAt", item.CreatedAt.ToString("o"))
            );

            if (item.Guild != null)
                element.Add(new XAttribute("GuildId", item.GuildId!));

            if (item.ProcessedGuildUser != null)
                element.Add(new XAttribute("ProcessedUserId", item.ProcessedUserId!));

            if (!string.IsNullOrEmpty(item.DiscordAuditLogItemId))
                element.Add(new XAttribute("DiscordAuditLogItemId", item.DiscordAuditLogItemId));

            if (!string.IsNullOrEmpty(item.Data))
                element.Add(new XElement("Data", item.Data));

            if (item.GuildChannel != null)
                element.Add(new XAttribute("ChannelId", item.ChannelId!));

            if (item.Files.Count > 0)
            {
                var files = new XElement("Files", new XAttribute("Count", item.Files.Count));

                foreach (var fileEntity in item.Files)
                {
                    var file = new XElement("File");

                    file.Add(
                        new XAttribute("Id", fileEntity.Id),
                        new XAttribute("Filename", fileEntity.Filename),
                        new XAttribute("Size", fileEntity.Size)
                    );

                    files.Add(file);
                }

                element.Add(files);
            }

            logRoot.Add(element);
            repository.Remove(item);
        }

        context.Result = $"Items: {data.Count}, Files: {data.Sum(o => o.Files.Count)} ({data.Sum(o => o.Files.Sum(x => x.Size))} B), XmlSize: {Encoding.UTF8.GetBytes(logRoot.ToString()).Length} B";
        await Helper.StoreDataAsync(logRoot, data.SelectMany(o => o.Files), "AuditLog");
        await repository.CommitAsync();
    }
}
