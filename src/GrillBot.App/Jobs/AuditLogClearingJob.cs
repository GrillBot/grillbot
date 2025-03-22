using System.IO.Compression;
using GrillBot.App.Helpers;
using GrillBot.App.Jobs.Abstractions;
using GrillBot.Common.FileStorage;
using GrillBot.Core.Services.AuditLog;
using GrillBot.Core.Services.AuditLog.Models.Events;
using GrillBot.Core.Services.AuditLog.Models.Response;
using GrillBot.Database.Entity;
using GrillBot.Database.Services.Repository;
using Quartz;

namespace GrillBot.App.Jobs;

[DisallowConcurrentExecution]
public class AuditLogClearingJob : ArchivationJobBase
{
    private IAuditLogServiceClient AuditLogServiceClient { get; }

    public AuditLogClearingJob(IServiceProvider serviceProvider, IAuditLogServiceClient auditLogServiceClient) : base(serviceProvider)
    {
        AuditLogServiceClient = auditLogServiceClient;
    }

    protected override async Task RunAsync(IJobExecutionContext context)
    {
        var archivationResult = await AuditLogServiceClient.CreateArchivationDataAsync();
        if (archivationResult is null)
            return;

        var jsonData = JObject.Parse(archivationResult.Content);

        using var repository = DatabaseBuilder.CreateRepository();

        await ProcessGuildsAsync(repository, archivationResult.GuildIds, jsonData);
        await ProcessChannelsAsync(repository, archivationResult.ChannelIds, jsonData);
        await ProcessUsersAsync(repository, archivationResult.UserIds, jsonData);

        var zipSize = await StoreDataAsync(jsonData, archivationResult.Files);
        var xmlSize = Encoding.UTF8.GetBytes(jsonData.ToString(Formatting.None)).Length.Bytes().ToString();
        var formattedZipSize = zipSize.Bytes().ToString();

        var bulkDeletePayload = new BulkDeletePayload(archivationResult.Ids);
        await RabbitPublisher.PublishAsync(bulkDeletePayload, new());

        context.Result = BuildReport(archivationResult, xmlSize, formattedZipSize);
    }

    private static IEnumerable<JObject> TransformChannels(IEnumerable<GuildChannel?> channels)
    {
        return channels
            .Where(o => o is not null)
            .DistinctBy(o => $"{o!.ChannelId}/{o.GuildId}").Select(ch =>
            {
                var channel = new JObject
                {
                    ["Id"] = ch!.ChannelId,
                    ["Name"] = ch.Name,
                    ["Type"] = ch.ChannelType.ToString(),
                    ["GuildId"] = ch.GuildId
                };

                if (ch.UserPermissionsCount > 0)
                    channel["UserPermissionsCount"] = ch.UserPermissionsCount;
                if (ch.RolePermissionsCount > 0)
                    channel["RolePermissionsCount"] = ch.RolePermissionsCount;
                if (ch.Flags > 0)
                    channel["Flags"] = ch.Flags;
                if (!string.IsNullOrEmpty(ch.ParentChannelId))
                    channel["ParentChannelId"] = ch.ParentChannelId;

                return channel;
            });
    }

    private async Task<long> StoreDataAsync(JObject json, IEnumerable<string> files)
    {
        var jsonBaseName = $"AuditLog_{DateTime.Now:yyyyMMdd_HHmmss}";
        var temporaryPath = Path.GetTempPath();
        var zipName = $"{jsonBaseName}.zip";
        var zipPath = Path.Combine(temporaryPath, zipName);
        long archiveSize;

        try
        {
            using (var zipArchive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                await AddJsonToZipAsync(zipArchive, json, $"{jsonBaseName}.json");
                await AddFilesToArchiveAsync(files, zipArchive);
            }

            await using var reader = File.OpenRead(zipPath);
            var archiveManager = await BlobManagerFactoryHelper.CreateAsync(BlobConstants.AuditLogArchives);
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

    private async Task AddFilesToArchiveAsync(IEnumerable<string> files, ZipArchive archive)
    {
        if (!files.Any())
            return;

        var manager = await BlobManagerFactoryHelper.CreateAsync(BlobConstants.AuditLogDeletedAttachments);
        var legacyManager = await BlobManagerFactoryHelper.CreateLegacyAsync();

        foreach (var file in files)
        {
            var fileContent = await manager.DownloadAsync(file);
            fileContent ??= await legacyManager.DownloadAsync(file);
            if (fileContent is null)
                continue;

            var entry = archive.CreateEntry(file);
            entry.LastWriteTime = DateTimeOffset.UtcNow;

            await using var ms = new MemoryStream(fileContent);
            await using var archiveStream = entry.Open();
            await ms.CopyToAsync(archiveStream);
        }
    }

    private static async Task ProcessGuildsAsync(GrillBotRepository repository, List<string> guildIds, JObject json)
    {
        var guilds = new List<Guild>();
        foreach (var guildChunk in guildIds.Chunk(100))
            guilds.AddRange(await repository.Guild.GetGuildsByIdsAsync(guildChunk.ToList()));
        json["Guilds"] = new JArray(TransformGuilds(guilds));
    }

    private static async Task ProcessChannelsAsync(GrillBotRepository repository, List<string> channelIds, JObject json)
    {
        var channels = new List<GuildChannel?>();
        foreach (var channelChunk in channelIds.Chunk(100))
            channels.AddRange(await repository.Channel.GetChannelsByIdsAsync(channelChunk.ToList()));

        json["Channels"] = new JArray(TransformChannels(channels));
    }

    private static async Task ProcessUsersAsync(GrillBotRepository repository, List<string> userIds, JObject json)
    {
        var users = new List<User?>();
        foreach (var userChunk in userIds.Chunk(100))
            users.AddRange(await repository.User.GetUsersByIdsAsync(userChunk.ToList()));

        json["Users"] = new JArray(TransformUsers(users));
    }

    private static string BuildReport(ArchivationResult result, string jsonSize, string zipSize)
    {
        var totalFilesSize = result.TotalFilesSize.Bytes().ToString();

        var builder = new StringBuilder()
            .AppendFormat("Items: {0}, Files: {1} ({2}), JsonSize: {3}, ZipSize: {4}", result.ItemsCount, result.Files.Count, totalFilesSize, jsonSize, zipSize)
            .AppendLine()
            .AppendLine();

        builder.AppendLine("Archived types: (");
        foreach (var type in result.PerType)
            builder.AppendFormat("{0}{1}: {2}", Indent, type.Key, type.Value).AppendLine();
        builder.AppendLine(")");

        builder.AppendLine("Archived months: (");
        foreach (var month in result.PerMonths)
            builder.AppendFormat("{0}{1}: {2}", Indent, month.Key, month.Value).AppendLine();
        builder.Append(')');

        return builder.ToString();
    }
}
