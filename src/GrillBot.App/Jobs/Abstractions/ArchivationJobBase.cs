using System.IO.Compression;
using System.Xml.Linq;
using GrillBot.App.Helpers;
using GrillBot.App.Infrastructure.Jobs;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Jobs.Abstractions;

public abstract class ArchivationJobBase : Job
{
    protected BlobManagerFactoryHelper BlobManagerFactoryHelper { get; }
    protected GrillBotDatabaseBuilder DatabaseBuilder { get; }

    protected ArchivationJobBase(IServiceProvider serviceProvider) : base(serviceProvider)
    {
        BlobManagerFactoryHelper = serviceProvider.GetRequiredService<BlobManagerFactoryHelper>();
        DatabaseBuilder = serviceProvider.GetRequiredService<GrillBotDatabaseBuilder>();
    }

    protected static IEnumerable<XAttribute> CreateMetadata(int count)
    {
        yield return new XAttribute("CreatedAt", DateTime.Now.ToString("o"));
        yield return new XAttribute("Count", count);
    }

    protected static IEnumerable<XElement> TransformGuilds(IEnumerable<Database.Entity.Guild?> guilds)
    {
        return guilds
            .Where(o => o != null)
            .DistinctBy(o => o!.Id)
            .Select(o => new XElement("Guild", new XAttribute("Id", o!.Id), new XAttribute("Name", o.Name)));
    }

    protected static IEnumerable<XElement> TransformUsers(IEnumerable<Database.Entity.User?> users)
        => users.Where(o => o is not null).DistinctBy(o => o!.Id).Select(u => TransformUser(u!));

    protected static IEnumerable<XElement> TransformGuildUsers(IEnumerable<Database.Entity.GuildUser> guildUsers)
    {
        return guildUsers.DistinctBy(o => $"{o.UserId}/{o.GuildId}").Select(u =>
        {
            var user = TransformUser(u.User!);
            user.Name = "GuildUser";

            user.Add(new XAttribute("GuildId", u.GuildId));
            user.Attribute("FullName")!.Value = u.DisplayName ?? "";

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
            new XAttribute("FullName", user.Username)
        );

        if (user.Flags > 0)
            element.Add(new XAttribute("Flags", user.Flags));

        return element;
    }

    protected static async Task AddXmlToZipAsync(ZipArchive archive, XElement xml, string xmlName)
    {
        var entry = archive.CreateEntry(xmlName);
        entry.LastWriteTime = DateTimeOffset.Now;

        await using var entryStream = entry.Open();
        await xml.SaveAsync(entryStream, SaveOptions.OmitDuplicateNamespaces | SaveOptions.DisableFormatting, CancellationToken.None);
    }
}
