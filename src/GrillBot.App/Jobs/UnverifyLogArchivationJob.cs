using GrillBot.App.Helpers;
using GrillBot.App.Jobs.Abstractions;
using GrillBot.Common.FileStorage;
using GrillBot.Database.Entity;
using GrillBot.Database.Services.Repository;
using Quartz;
using System.IO.Compression;
using UnverifyService;
using UnverifyService.Models.Response;

namespace GrillBot.App.Jobs;

[DisallowConcurrentExecution]
public class UnverifyLogArchivationJob(
    IServiceProvider serviceProvider,
    IUnverifyServiceClient _unverifyClient
) : ArchivationJobBase(serviceProvider)
{
    protected override async Task RunAsync(IJobExecutionContext context)
    {
        var archivationData = await CreateArchivationData(context.CancellationToken);
        if (archivationData is null)
            return;

        var jsonData = JObject.Parse(archivationData.Content);
        using var repository = DatabaseBuilder.CreateRepository();

        await ProcessUsersAsync(repository, archivationData.UserIds, jsonData);
        await ProcessChannelsAsync(repository, archivationData.ChannelIds, jsonData);
        await ProcessGuildsAsync(repository, archivationData.GuildIds, jsonData);

        var archiveSize = await SaveDataAsync(jsonData);
        var jsonSize = Encoding.UTF8.GetBytes(jsonData.ToString(Formatting.None)).Length.Bytes().ToString();
        var zipSize = archiveSize.Bytes().ToString();

        context.Result = BuildReport(jsonSize, zipSize, archivationData);
    }

    private async Task<ArchivationResult?> CreateArchivationData(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _unverifyClient.CreateArchivationDataAsync(cancellationToken);
        }
        catch (Refit.ApiException ex) when (ex.StatusCode == HttpStatusCode.NoContent)
        {
            return null;
        }
    }

    private static async Task ProcessUsersAsync(GrillBotRepository repository, List<ulong> userIds, JObject json)
    {
        var users = new List<User?>();
        foreach (var userChunk in userIds.Chunk(100))
            users.AddRange(await repository.User.GetUsersByIdsAsync([.. userChunk.Select(o => o.ToString())]));

        json["Users"] = TransformUsers(users);
    }

    private static async Task ProcessChannelsAsync(GrillBotRepository repository, List<ulong> channelIds, JObject json)
    {
        var channels = new List<GuildChannel?>();
        foreach (var channelChunk in channelIds.Chunk(100))
            channels.AddRange(await repository.Channel.GetChannelsByIdsAsync([.. channelChunk.Select(o => o.ToString())]));

        json["Channels"] = new JArray(TransformChannels(channels));
    }

    private static async Task ProcessGuildsAsync(GrillBotRepository repository, List<ulong> guildIds, JObject json)
    {
        var guilds = new List<Guild>();
        foreach (var guildChunk in guildIds.Chunk(100))
            guilds.AddRange(await repository.Guild.GetGuildsByIdsAsync([.. guildChunk.Select(o => o.ToString())]));
        json["Guilds"] = new JArray(TransformGuilds(guilds));
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

    private static string BuildReport(string jsonSize, string zipSize, ArchivationResult data)
    {
        var builder = new StringBuilder()
            .AppendFormat("Items: {0}, JsonSize: {1}, ZipSize: {0}", jsonSize, zipSize)
            .AppendLine()
            .AppendLine();

        builder.AppendLine("Archived types: (");
        foreach (var type in data.PerType)
            builder.AppendFormat("{0}{1}: {2}", Indent, type.Key, type.Value).AppendLine();
        builder.AppendLine(")");

        builder.AppendLine("Archived months: (");
        foreach (var month in data.PerMonths)
            builder.AppendFormat("{0}{1}: {2}", Indent, month.Key, month.Value).AppendLine();
        builder.Append(')');

        return builder.ToString();
    }
}
