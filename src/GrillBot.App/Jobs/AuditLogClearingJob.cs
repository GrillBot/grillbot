using System.IO.Compression;
using System.Xml.Linq;
using GrillBot.App.Infrastructure.Jobs;
using GrillBot.Common.FileStorage;
using GrillBot.Common.Services.FileService;
using GrillBot.Database.Entity;
using Quartz;

namespace GrillBot.App.Jobs;

[DisallowConcurrentExecution]
public class AuditLogClearingJob : Job
{
    private GrillBotDatabaseBuilder DbFactory { get; }
    private FileStorageFactory FileStorage { get; }
    private IFileServiceClient FileServiceClient { get; }

    public AuditLogClearingJob(GrillBotDatabaseBuilder dbFactory, IServiceProvider serviceProvider, FileStorageFactory fileStorage, IFileServiceClient fileServiceClient) : base(serviceProvider)
    {
        DbFactory = dbFactory;
        FileStorage = fileStorage;
        FileServiceClient = fileServiceClient;
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
        logRoot.Add(TransformGuilds(data));

        var guildUsers = data.Select(o => o.ProcessedGuildUser).Where(o => o != null).DistinctBy(o => $"{o!.UserId}/{o.GuildId}").ToList();
        var users = data.Select(o => o.ProcessedUser).Where(o => o != null && guildUsers.All(x => x!.UserId != o.Id)).DistinctBy(o => o!.Id).ToList();

        logRoot.Add(TransformGuildUsers(guildUsers!));
        logRoot.Add(TransformUsers(users!));
        logRoot.Add(TransformChannels(data));

        foreach (var item in data)
        {
            var element = new XElement("Item");

            element.Add(
                new XAttribute("Id", item.Id),
                new XAttribute("Type", item.Type.ToString()),
                new XAttribute("CreatedAt", item.CreatedAt.ToString("o")),
                new XElement("Data", string.IsNullOrEmpty(item.Data) ? "-" : item.Data)
            );

            if (item.Guild != null)
                element.Add(new XAttribute("GuildId", item.GuildId!));

            if (item.ProcessedGuildUser != null)
                element.Add(new XAttribute("ProcessedUserId", item.ProcessedUserId!));

            if (!string.IsNullOrEmpty(item.DiscordAuditLogItemId))
                element.Add(new XAttribute("DiscordAuditLogItemId", item.DiscordAuditLogItemId));

            if (item.GuildChannel != null)
                element.Add(new XAttribute("ChannelId", item.ChannelId!));

            foreach (var fileEntity in item.Files)
            {
                var file = new XElement("File");

                file.Add(
                    new XAttribute("Id", fileEntity.Id),
                    new XAttribute("Filename", fileEntity.Filename),
                    new XAttribute("Size", fileEntity.Size)
                );

                element.Add(file);
            }

            logRoot.Add(element);
            repository.Remove(item);
        }

        var totalFilesSize = data.SelectMany(o => o.Files).Sum(x => x.Size).Bytes().ToString();
        var xmlSize = Encoding.UTF8.GetBytes(logRoot.ToString()).Length.Bytes().ToString();

        context.Result = $"Items: {data.Count}, Files: {data.Sum(o => o.Files.Count)} ({totalFilesSize}), XmlSize: {xmlSize}";
        await StoreDataAsync(logRoot, data, "AuditLog");
        await repository.CommitAsync();
    }

    private static IEnumerable<XElement> TransformGuilds(IEnumerable<AuditLogItem> guilds)
    {
        return guilds
            .Select(o => o.Guild)
            .Where(o => o != null)
            .DistinctBy(o => o!.Id)
            .Select(o => new XElement("Guild", new XAttribute("Id", o!.Id), new XAttribute("Name", o.Name)));
    }

    private static IEnumerable<XAttribute> CreateMetadata(int count)
    {
        return new[]
        {
            new XAttribute("CreatedAt", DateTime.Now.ToString("o")),
            new XAttribute("Count", count)
        };
    }

    private static IEnumerable<XElement> TransformUsers(IEnumerable<User> users)
        => users.DistinctBy(o => o.Id).Select(TransformUser);

    private static IEnumerable<XElement> TransformGuildUsers(IEnumerable<GuildUser> guildUsers)
    {
        return guildUsers.DistinctBy(o => $"{o.UserId}/{o.GuildId}").Select(u =>
        {
            var user = TransformUser(u.User!);
            user.Name = "GuildUser";

            user.Add(new XAttribute("GuildId", u.GuildId));
            user.Attribute("FullName")!.Value = u.FullName();

            if (!string.IsNullOrEmpty(u.UsedInviteCode))
                user.Add(new XAttribute("UsedInviteCode", u.UsedInviteCode));

            return user;
        });
    }

    private static XElement TransformUser(User user)
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

    private static IEnumerable<XElement> TransformChannels(IEnumerable<AuditLogItem> channels)
    {
        return channels
            .Select(o => o.GuildChannel)
            .Where(o => o != null)
            .DistinctBy(o => $"{o!.ChannelId}/{o.GuildId}").Select(ch =>
            {
                var channel = new XElement("Channel");
                channel.Add(
                    new XAttribute("Id", ch!.ChannelId),
                    new XAttribute("Name", ch.Name),
                    new XAttribute("Type", ch.ChannelType.ToString()),
                    new XAttribute("GuildId", ch.GuildId)
                );

                if (ch.UserPermissionsCount > 0)
                    channel.Add(new XAttribute("UserPermissionsCount", ch.UserPermissionsCount));
                if (ch.RolePermissionsCount > 0)
                    channel.Add(new XAttribute("RolePermissionsCount", ch.RolePermissionsCount));
                if (ch.Flags > 0)
                    channel.Add(new XAttribute("Flags", ch.Flags));
                if (!string.IsNullOrEmpty(ch.ParentChannelId))
                    channel.Add(new XAttribute("ParentChannelId", ch.ParentChannelId));

                return channel;
            });
    }

    private async Task StoreDataAsync(XElement xml, IEnumerable<AuditLogItem> logItems, string prefix)
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
        await AddFilesToArchiveAsync(logItems, storage, archive);

        File.Delete(fileinfo.FullName);
    }

    private async Task AddFilesToArchiveAsync(IEnumerable<AuditLogItem> logs, IFileStorage storage, ZipArchive archive)
    {
        foreach (var log in logs.Where(o => o.Files.Count > 0))
        {
            foreach (var file in log.Files.Select(o => o.Filename))
            {
                var attachmentFile = await storage.GetFileInfoAsync("DeletedAttachments", file);
                if (!attachmentFile.Exists)
                {
                    // If file not exists, try read it and delete from file service.
                    var fileContent = await FileServiceClient.DownloadFileAsync(file);
                    if (fileContent == null) continue;

                    var entry = archive.CreateEntry(file);
                    entry.LastWriteTime = log.CreatedAt;

                    using var ms = new MemoryStream(fileContent);
                    await using var archiveStream = entry.Open();
                    await ms.CopyToAsync(archiveStream);

                    await FileServiceClient.DeleteFileAsync(file);
                }
                else
                {
                    archive.CreateEntryFromFile(attachmentFile.FullName, file, CompressionLevel.Optimal);
                    attachmentFile.Delete();
                }
            }
        }
    }
}
