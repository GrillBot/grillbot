using System.IO.Compression;
using System.Xml.Linq;
using GrillBot.App.Helpers;
using GrillBot.App.Jobs.Abstractions;
using GrillBot.Common.FileStorage;
using Quartz;

namespace GrillBot.App.Jobs;

[DisallowConcurrentExecution]
public class UnverifyLogArchivationJob : ArchivationJobBase
{
    private IConfiguration Configuration { get; }

    public UnverifyLogArchivationJob(IServiceProvider serviceProvider, IConfiguration configuration) : base(serviceProvider)
    {
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
            .Select(o => o.FromUser!)
            .Concat(data.Select(o => o.ToUser!))
            .Where(o => o is not null)
            .DistinctBy(o => $"{o!.UserId}/{o.GuildId}");
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

        var archiveSize = await SaveDataAsync(logRoot);
        await repository.CommitAsync();

        var xmlSize = Encoding.UTF8.GetBytes(logRoot.ToString()).Length.Bytes().ToString();
        var zipSize = archiveSize.Bytes().ToString();

        context.Result = BuildReport(xmlSize, zipSize, data);
    }

    private async Task<long> SaveDataAsync(XElement xml)
    {
        var xmlBaseName = $"UnverifyLog_{DateTime.Now:yyyyMMdd_HHmmss}";
        var temporaryPath = Path.GetTempPath();
        var zipName = $"{xmlBaseName}.zip";
        var zipPath = Path.Combine(temporaryPath, zipName);
        long archiveSize;

        try
        {
            using (var zipArchive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                await AddXmlToZipAsync(zipArchive, xml, $"{xmlBaseName}.xml");
            }

            using var reader = File.OpenRead(zipPath);
            var archiveManager = await BlobManagerFactoryHelper.CreateAsync(BlobConstants.UnverifyLogArchives);
            await archiveManager.UploadAsync(zipName, reader);
        }
        finally
        {
            archiveSize = new FileInfo(zipPath).Length;

            if (File.Exists(zipPath))
                File.Delete(zipPath);
        }

        return archiveSize;
    }

    private static string BuildReport(string xmlSize, string zipSize, List<Database.Entity.UnverifyLog> data)
    {
        var builder = new StringBuilder()
            .AppendFormat("Items: {0}, XmlSize: {1}, ZipSize: {0}", xmlSize, zipSize)
            .AppendLine()
            .AppendLine();

        builder.AppendLine("Archived types: (");
        foreach (var type in data.GroupBy(o => o.Operation.ToString()).OrderBy(o => o.Key))
            builder.AppendFormat("{0}{1}: {2}", Indent, type.Key, type.Count()).AppendLine();
        builder.AppendLine(")");

        builder.AppendLine("Archived months: (");
        foreach (var month in data.OrderBy(o => o.CreatedAt).GroupBy(o => $"{o.CreatedAt.Month}-{o.CreatedAt.Year}"))
            builder.AppendFormat("{0}{1}: {2}", Indent, month.Key, month.Count()).AppendLine();
        builder.Append(')');

        return builder.ToString();
    }
}
