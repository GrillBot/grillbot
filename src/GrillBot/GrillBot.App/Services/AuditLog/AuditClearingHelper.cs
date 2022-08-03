using System.IO.Compression;
using System.Xml.Linq;
using GrillBot.Common.FileStorage;
using DbEntity = GrillBot.Database.Entity;

namespace GrillBot.App.Services.AuditLog;

public class AuditClearingHelper
{
    private FileStorageFactory FileStorage { get; }

    public AuditClearingHelper(FileStorageFactory fileStorage)
    {
        FileStorage = fileStorage;
    }

    public static IEnumerable<XElement> TransformGuilds(IEnumerable<DbEntity.Guild> guilds)
    {
        return guilds.DistinctBy(o => o.Id)
            .Select(o => new XElement("Guild", new XAttribute("Id", o.Id), new XAttribute("Name", o.Name)));
    }

    public static IEnumerable<XAttribute> CreateMetadata(int count)
    {
        return new[]
        {
            new XAttribute("CreatedAt", DateTime.Now.ToString("o")),
            new XAttribute("Count", count)
        };
    }

    public static IEnumerable<XElement> TransformUsers(IEnumerable<DbEntity.User> users)
    {
        return users.DistinctBy(o => o.Id).Select(TransformUser);
    }

    public static IEnumerable<XElement> TransformGuildUsers(IEnumerable<DbEntity.GuildUser> guildUsers)
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

    private static XElement TransformUser(DbEntity.User user)
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

    public static IEnumerable<XElement> TransformChannels(IEnumerable<DbEntity.GuildChannel> channels)
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
                new XAttribute("RolePermissionsCount", ch.RolePermissionsCount)
            );

            if (!string.IsNullOrEmpty(ch.ParentChannelId))
                channel.Add(new XAttribute("ParentChannelId", ch.ParentChannelId));

            return channel;
        });
    }

    public async Task StoreDataAsync(XElement xml, IEnumerable<DbEntity.AuditLogFileMeta> files, string prefix)
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
