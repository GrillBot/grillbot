using System.IO.Compression;
using System.Xml.Linq;
using GrillBot.App.Infrastructure.Jobs;
using GrillBot.Common.FileStorage;
using Quartz;

namespace GrillBot.App.Jobs;

[DisallowConcurrentExecution]
public class AuditLogClearingJob : Job
{
    private GrillBotDatabaseBuilder DbFactory { get; }
    private FileStorageFactory FileStorage { get; }

    public AuditLogClearingJob(GrillBotDatabaseBuilder dbFactory, IServiceProvider serviceProvider, FileStorageFactory fileStorage) : base(serviceProvider)
    {
        DbFactory = dbFactory;
        FileStorage = fileStorage;
    }

    protected override async Task RunAsync(IJobExecutionContext context)
    {
        var expirationDate = DateTime.Now.AddYears(-1);

        await using var repository = DbFactory.CreateRepository();
        if (!await repository.AuditLog.ExistsExpiredItemAsync(expirationDate))
            return;

        var data = await repository.AuditLog.GetExpiredDataAsync(expirationDate);
        var logRoot = new XElement("AuditLogBackup");

        logRoot.Add(CreateMetadata(data.Count));
        logRoot.Add(TransformGuilds(data.Select(o => o.Guild)));

        var guildUsers = data.Select(o => o.ProcessedGuildUser).Where(o => o != null).DistinctBy(o => $"{o.UserId}/{o.GuildId}").ToList();
        var users = data.Select(o => o.ProcessedUser).Where(o => o != null && guildUsers.All(x => x.UserId != o.Id)).DistinctBy(o => o.Id).ToList();

        logRoot.Add(TransformGuildUsers(guildUsers));
        logRoot.Add(TransformUsers(users));
        logRoot.Add(TransformChannels(data.Select(o => o.GuildChannel)));

        foreach (var item in data)
        {
            var element = new XElement("Item");

            element.Add(
                new XAttribute("Id", item.Id),
                new XAttribute("Type", item.Type.ToString()),
                new XAttribute("CreatedAt", item.CreatedAt.ToString("o")),
                new XAttribute("Data", string.IsNullOrEmpty(item.Data) ? "-" : item.Data)
            );

            if (item.Guild != null)
                element.Add(new XAttribute("GuildId", item.GuildId!));

            if (item.ProcessedGuildUser != null)
                element.Add(new XAttribute("ProcessedUserId", item.ProcessedUserId!));

            if (!string.IsNullOrEmpty(item.DiscordAuditLogItemId))
                element.Add(new XAttribute("DiscordAuditLogItemId", item.DiscordAuditLogItemId));

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
        await StoreDataAsync(logRoot, data.SelectMany(o => o.Files), "AuditLog");
        await repository.CommitAsync();
    }

    private static IEnumerable<XElement> TransformGuilds(IEnumerable<Database.Entity.Guild> guilds)
    {
        return guilds.DistinctBy(o => o.Id)
            .Select(o => new XElement("Guild", new XAttribute("Id", o.Id), new XAttribute("Name", o.Name)));
    }

    private static IEnumerable<XAttribute> CreateMetadata(int count)
    {
        return new[]
        {
            new XAttribute("CreatedAt", DateTime.Now.ToString("o")),
            new XAttribute("Count", count)
        };
    }

    private static IEnumerable<XElement> TransformUsers(IEnumerable<Database.Entity.User> users)
        => users.DistinctBy(o => o.Id).Select(TransformUser);

    private static IEnumerable<XElement> TransformGuildUsers(IEnumerable<Database.Entity.GuildUser> guildUsers)
    {
        return guildUsers.DistinctBy(o => $"{o.UserId}/{o.GuildId}").Select(u =>
        {
            var user = TransformUser(u.User);
            user.Name = "GuildUser";

            user.Add(new XAttribute("GuildId", u.GuildId));
            user.Attribute("FullName")!.Value = u.FullName();

            if (!string.IsNullOrEmpty(u.UsedInviteCode))
                user.Add(new XAttribute("UsedInviteCode", u.UsedInviteCode));

            return user;
        });
    }

    private static XElement TransformUser(Database.Entity.User user)
    {
        var element = new XElement(
            "User",
            new XAttribute("Id", user.Id),
            new XAttribute("FullName", user.FullName())
        );

        if (user.Flags > 0)
            element.Add(new XAttribute("Flags", user.Flags));

        return element;
    }

    private static IEnumerable<XElement> TransformChannels(IEnumerable<Database.Entity.GuildChannel> channels)
    {
        return channels.Where(o => o != null).DistinctBy(o => $"{o.ChannelId}/{o.GuildId}").Select(ch =>
        {
            var channel = new XElement("Channel");
            channel.Add(
                new XAttribute("Id", ch.ChannelId),
                new XAttribute("Name", ch.Name),
                new XAttribute("Type", ch.ChannelType.ToString()),
                new XAttribute("GuildId", ch.GuildId),
                new XAttribute("UserPermissionsCount", ch.UserPermissionsCount),
                new XAttribute("RolePermissionsCount", ch.RolePermissionsCount),
                new XAttribute("Flags", ch.Flags)
            );

            if (!string.IsNullOrEmpty(ch.ParentChannelId))
                channel.Add(new XAttribute("ParentChannelId", ch.ParentChannelId));

            return channel;
        });
    }

    private async Task StoreDataAsync(XElement xml, IEnumerable<Database.Entity.AuditLogFileMeta> files, string prefix)
    {
        var storage = FileStorage.Create("Audit");
        var backupFilename = $"{prefix}_{DateTime.Now:yyyyMMdd_HHmmss}.xml";
        var fileinfo = await storage.GetFileInfoAsync("Clearing", backupFilename);

        await using (var stream = fileinfo.OpenWrite())
        {
            await xml.SaveAsync(stream, SaveOptions.OmitDuplicateNamespaces | SaveOptions.DisableFormatting, CancellationToken.None);
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
