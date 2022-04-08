using GrillBot.App.Infrastructure.Jobs;
using GrillBot.App.Services.Discord;
using GrillBot.App.Services.FileStorage;
using GrillBot.App.Services.Logging;
using Quartz;
using System.IO.Compression;
using System.Xml.Linq;

namespace GrillBot.App.Services.AuditLog;

[DisallowConcurrentExecution]
public class AuditLogClearingJob : Job
{
    private GrillBotContextFactory DbFactory { get; }
    private IConfiguration Configuration { get; }
    private FileStorageFactory FileStorage { get; }

    private int IntervalMonths
        => Configuration.GetValue<int>("CleanupIntervalMonths");

    public AuditLogClearingJob(LoggingService loggingService, AuditLogService auditLogService, IDiscordClient discordClient,
        GrillBotContextFactory dbFactory, IConfiguration configuration, FileStorageFactory fileStorage, DiscordInitializationService initializationService)
        : base(loggingService, auditLogService, discordClient, initializationService)
    {
        Configuration = configuration.GetSection("AuditLog");
        FileStorage = fileStorage;
        DbFactory = dbFactory;
    }

    public override async Task RunAsync(IJobExecutionContext context)
    {
        using var dbContext = DbFactory.Create();
        var beforeDate = DateTime.Now.AddMonths(-IntervalMonths);

        var query = dbContext.AuditLogs.AsQueryable()
            .Include(o => o.Files)
            .Include(o => o.Guild)
            .Include(o => o.GuildChannel)
            .Include(o => o.ProcessedGuildUser).ThenInclude(o => o.User)
            .Include(o => o.ProcessedUser)
            .Where(o => o.CreatedAt <= beforeDate);

        if (!await query.AnyAsync(context.CancellationToken))
        {
            context.Result = "NoDataToArchivation";
            return;
        }

        var data = await query.ToListAsync(context.CancellationToken);
        var logRoot = new XElement("AuditLogBackup");

        logRoot.Add(
            new XAttribute("CreatedAt", DateTime.Now.ToString("o")),
            new XAttribute("Count", data.Count)
        );

        var guilds = data.Select(o => o.Guild)
            .Where(o => o != null)
            .GroupBy(o => o.Id)
            .Select(o => o.First())
            .Select(o => new XElement("Guild", new XAttribute("Id", o.Id), new XAttribute("Name", o.Name)));

        logRoot.Add(guilds);

        var guildUsers = data.Select(o => o.ProcessedGuildUser).Where(o => o != null).GroupBy(o => o.UserId).Select(o => o.First()).ToList();
        var users = data.Select(o => o.ProcessedUser).Where(o => o != null && !guildUsers.Any(x => x.UserId == o.Id)).GroupBy(o => o.Id)
            .Select(o => o.First()).ToList();

        logRoot.Add(
            guildUsers.Select(o =>
            {
                var user = new XElement("ProcessedGuildUser");

                user.Add(
                    new XAttribute("Id", o.UserId),
                    new XAttribute("GuildId", o.GuildId),
                    new XAttribute("UserFlags", o.User.Flags),
                    new XAttribute("Username", o.User.Username),
                    new XAttribute("Discriminator", o.User.Discriminator)
                );

                if (!string.IsNullOrEmpty(o.UsedInviteCode))
                    user.Add(new XAttribute("UsedInviteCode", o.UsedInviteCode));

                if (!string.IsNullOrEmpty(o.Nickname))
                    user.Add(new XAttribute("Nickname", o.Nickname));

                return user;
            }),
            users.Select(o =>
            {
                var user = new XElement("ProcessedUser");

                user.Add(
                    new XAttribute("Id", o.Id),
                    new XAttribute("UserFlags", o.Flags),
                    new XAttribute("Username", o.Username),
                    new XAttribute("Discriminator", o.Discriminator)
                );

                return user;
            })
        );

        var channels = data.Select(o => o.GuildChannel)
            .Where(o => o != null)
            .GroupBy(o => o.ChannelId)
            .Select(o => o.First())
            .Select(o =>
            {
                var channel = new XElement("Channel");

                channel.Add(
                    new XAttribute("Id", o.ChannelId),
                    new XAttribute("Name", o.Name),
                    new XAttribute("Type", o.ChannelType.ToString())
                );

                if (!string.IsNullOrEmpty(o.ParentChannelId))
                    channel.Add(new XAttribute("ParentChannelId", o.ParentChannelId));

                return channel;
            });

        logRoot.Add(channels);

        foreach (var item in data)
        {
            var element = new XElement("Item");

            element.Add(
                new XAttribute("Id", item.Id),
                new XAttribute("Type", item.Type.ToString()),
                new XAttribute("CreatedAt", item.CreatedAt.ToString("o"))
            );

            if (item.Guild != null)
                element.Add(new XAttribute("GuildId", item.GuildId));

            if (item.ProcessedGuildUser != null)
                element.Add(new XAttribute("ProcessedUserId", item.ProcessedUserId));

            if (!string.IsNullOrEmpty(item.DiscordAuditLogItemId))
                element.Add(new XAttribute("DiscordAuditLogItemId", item.DiscordAuditLogItemId));

            if (!string.IsNullOrEmpty(item.Data))
                element.Add(new XElement("Data", item.Data));

            if (item.GuildChannel != null)
                element.Add(new XAttribute("ChannelId", item.ChannelId));

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
            dbContext.Remove(item);
        }

        context.Result = $"Items: {data.Count}, Files: {data.Sum(o => o.Files.Count)} ({data.Sum(o => o.Files.Sum(x => x.Size))} B), XmlSize: {Encoding.UTF8.GetBytes(logRoot.ToString()).Length} B";
        await StoreDataAsync(logRoot, data.SelectMany(o => o.Files), context.CancellationToken);
        await dbContext.SaveChangesAsync(context.CancellationToken);
    }

    private async Task StoreDataAsync(XElement logRoot, IEnumerable<Database.Entity.AuditLogFileMeta> files, CancellationToken cancellationToken)
    {
        var storage = FileStorage.Create("Audit");
        var backupFilename = $"AuditLog_{DateTime.Now:yyyyMMdd_HHmmss}.xml";
        var fileinfo = await storage.GetFileInfoAsync("Clearing", backupFilename);

        using (var stream = fileinfo.OpenWrite())
        {
            await logRoot.SaveAsync(stream, SaveOptions.OmitDuplicateNamespaces | SaveOptions.DisableFormatting, cancellationToken);
        }

        var zipFilename = Path.ChangeExtension(fileinfo.FullName, ".zip");
        if (File.Exists(zipFilename)) File.Delete(zipFilename);
        using var archive = ZipFile.Open(zipFilename, ZipArchiveMode.Create);
        archive.CreateEntryFromFile(fileinfo.FullName, backupFilename, CompressionLevel.Optimal);

        foreach (var file in files.Select(o => o.Filename))
        {
            var attachmentFile = await storage.GetFileInfoAsync("DeletedAttachments", file);
            if (!attachmentFile.Exists) continue;

            archive.CreateEntryFromFile(attachmentFile.FullName, file, CompressionLevel.Optimal);
            attachmentFile.Delete();
        }

        File.Delete(fileinfo.FullName);
    }
}
