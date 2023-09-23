using System.IO.Compression;
using System.Xml.Linq;
using GrillBot.App.Jobs.Abstractions;
using GrillBot.Common.FileStorage;
using Quartz;

namespace GrillBot.App.Jobs;

[DisallowConcurrentExecution]
public class UnverifyLogArchivationJob : ArchivationJobBase
{
    private GrillBotDatabaseBuilder DatabaseBuilder { get; }
    private FileStorageFactory FileStorageFactory { get; }
    private IConfiguration Configuration { get; }

    public UnverifyLogArchivationJob(IServiceProvider serviceProvider, GrillBotDatabaseBuilder databaseBuilder, FileStorageFactory fileStorageFactory,
        IConfiguration configuration) : base(serviceProvider)
    {
        DatabaseBuilder = databaseBuilder;
        FileStorageFactory = fileStorageFactory;
        Configuration = configuration;
    }

    protected override async Task RunAsync(IJobExecutionContext context)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var expirationMilestone = DateTime.Now.AddYears(-2);
        var minimalCount = Configuration.GetValue<int>("Unverify:MinimalCountToArchivation");
        var countToArchivation = await repository.Unverify.GetCountForArchivationAsync(expirationMilestone);

        if (countToArchivation <= minimalCount)
            return;

        var data = await repository.Unverify.GetLogsForArchivationAsync(expirationMilestone);
        var logRoot = new XElement("UnverifyLog");

        logRoot.Add(CreateMetadata(data.Count));
        logRoot.Add(TransformGuilds(data.Select(o => o.Guild)));

        var users = data
            .Select(o => o.FromUser)
            .Concat(data.Select(o => o.ToUser))
            .Where(o => o != null)
            .DistinctBy(o => $"{o!.UserId}/{o.GuildId}")
            .Select(o => o!);
        logRoot.Add(TransformGuildUsers(users));

        foreach (var item in data)
        {
            var element = new XElement("Item");

            element.Add(
                new XAttribute("Id", item.Id),
                new XAttribute("Operation", item.Operation.ToString()),
                new XAttribute("GuildId", item.GuildId),
                new XAttribute("FromUserId", item.FromUserId),
                new XAttribute("ToUserId", item.ToUserId),
                new XAttribute("CreatedAt", item.CreatedAt.ToString("o")),
                new XElement("Data", item.Data)
            );

            logRoot.Add(element);
            repository.Remove(item);
        }

        await SaveDataAsync(logRoot);
        await repository.CommitAsync();

        var xmlSize = Encoding.UTF8.GetBytes(logRoot.ToString()).Length.Bytes().ToString();
        context.Result = $"Items: {data.Count}, XmlSize: {xmlSize}";
    }

    private async Task SaveDataAsync(XElement xml)
    {
        var storage = FileStorageFactory.Create("Unverify");
        var backupFilename = $"UnverifyLog_{DateTime.Now:yyyyMMdd_HHmmss}.xml";
        var fileinfo = await storage.GetFileInfoAsync(backupFilename);

        await using (var stream = fileinfo.OpenWrite())
        {
            await xml.SaveAsync(stream, SaveOptions.OmitDuplicateNamespaces | SaveOptions.DisableFormatting, CancellationToken.None);
        }

        var zipFilename = Path.ChangeExtension(fileinfo.FullName, ".zip");
        if (File.Exists(zipFilename)) File.Delete(zipFilename);

        using var archive = ZipFile.Open(zipFilename, ZipArchiveMode.Create);
        archive.CreateEntryFromFile(fileinfo.FullName, backupFilename, CompressionLevel.Optimal);

        File.Delete(fileinfo.FullName);
    }
}
