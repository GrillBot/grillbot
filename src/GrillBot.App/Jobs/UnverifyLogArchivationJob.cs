using System.IO.Compression;
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
        var logRoot = new JObject
        {
            ["CreatedAt"] = DateTime.Now.ToString("o"),
            ["Count"] = data.Count,
            ["Guilds"] = TransformGuilds(data.Select(o => o.Guild))
        };

        var users = data
            .Select(o => o.FromUser!)
            .Concat(data.Select(o => o.ToUser!))
            .Where(o => o is not null)
            .DistinctBy(o => $"{o!.UserId}/{o.GuildId}");
        logRoot.Add("Users", TransformGuildUsers(users));

        var items = new JArray();
        foreach (var item in data)
        {
            var jsonElement = new JObject
            {
                ["Id"] = item.Id,
                ["Operation"] = item.Operation.ToString(),
                ["GuildId"] = item.GuildId,
                ["FromUserId"] = item.FromUserId,
                ["ToUserId"] = item.ToUserId,
                ["CreatedAt"] = item.CreatedAt.ToString("o"),
                ["Data"] = item.Data
            };

            items.Add(jsonElement);
            repository.Remove(item);
        }

        logRoot.Add("Items", items);

        var archiveSize = await SaveDataAsync(logRoot);
        await repository.CommitAsync();

        var xmlSize = Encoding.UTF8.GetBytes(logRoot.ToString(Formatting.None)).Length.Bytes().ToString();
        var zipSize = archiveSize.Bytes().ToString();

        context.Result = BuildReport(xmlSize, zipSize, data);
    }

    private async Task<long> SaveDataAsync(JObject json)
    {
        var jsonBaseName = $"UnverifyLog_{DateTime.Now:yyyyMMdd_HHmmss}";
        var temporaryPath = Path.GetTempPath();
        var zipName = $"{jsonBaseName}.zip";
        var zipPath = Path.Combine(temporaryPath, zipName);
        long archiveSize;

        try
        {
            using (var zipArchive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                await AddJsonToZipAsync(zipArchive, json, $"{jsonBaseName}.json");
            }

            await using var reader = File.OpenRead(zipPath);
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

    private static string BuildReport(string jsonSize, string zipSize, List<Database.Entity.UnverifyLog> data)
    {
        var builder = new StringBuilder()
            .AppendFormat("Items: {0}, JsonSize: {1}, ZipSize: {0}", jsonSize, zipSize)
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
