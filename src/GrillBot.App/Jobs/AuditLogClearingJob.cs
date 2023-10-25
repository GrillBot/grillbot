﻿using System.IO.Compression;
using System.Xml.Linq;
using GrillBot.App.Helpers;
using GrillBot.App.Jobs.Abstractions;
using GrillBot.Common.FileStorage;
using GrillBot.Core.Services.AuditLog;
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
        var archivationResult = await AuditLogServiceClient.ProcessArchivationAsync();
        if (archivationResult is null)
            return;

        var xmlData = XElement.Parse(archivationResult.Xml);

        await using var repository = DatabaseBuilder.CreateRepository();

        await ProcessGuildsAsync(repository, archivationResult.GuildIds, xmlData);
        await ProcessChannelsAsync(repository, archivationResult.ChannelIds, xmlData);
        await ProcessUsersAsync(repository, archivationResult.UserIds, xmlData);

        var zipSize = await StoreDataAsync(xmlData, archivationResult.Files);
        var xmlSize = Encoding.UTF8.GetBytes(xmlData.ToString()).Length.Bytes().ToString();
        var formattedZipSize = zipSize.Bytes().ToString();

        await AuditLogServiceClient.BulkDeleteAsync(archivationResult.Ids);
        context.Result = BuildReport(archivationResult, xmlSize, formattedZipSize);
    }

    private static IEnumerable<XElement> TransformChannels(IEnumerable<GuildChannel?> channels)
    {
        return channels
            .Where(o => o is not null)
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

    private async Task<long> StoreDataAsync(XElement xml, IEnumerable<string> files)
    {
        var xmlBaseName = $"AuditLog_{DateTime.Now:yyyyMMdd_HHmmss}";
        var temporaryPath = Path.GetTempPath();
        var zipName = $"{xmlBaseName}.zip";
        var zipPath = Path.Combine(temporaryPath, zipName);
        long archiveSize;

        try
        {
            using (var zipArchive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
            {
                await AddXmlToZipAsync(zipArchive, xml, $"{xmlBaseName}.xml");
                await AddFilesToArchiveAsync(files, zipArchive);
            }

            using var reader = File.OpenRead(zipPath);
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
            var useLegacy = false;
            var fileContent = await manager.DownloadAsync(file);

            if (fileContent is null)
            {
                fileContent = await legacyManager.DownloadAsync(file);
                useLegacy = true;

                if (fileContent is null)
                    continue;
            }

            var entry = archive.CreateEntry(file);
            entry.LastWriteTime = DateTimeOffset.UtcNow;

            using var ms = new MemoryStream(fileContent);
            await using var archiveStream = entry.Open();
            await ms.CopyToAsync(archiveStream);

            if (useLegacy)
                await legacyManager.DeleteAsync(file);
            else
                await manager.DeleteAsync(file);
        }
    }

    private static async Task ProcessGuildsAsync(GrillBotRepository repository, List<string> guildIds, XContainer xmlData)
    {
        var guilds = new List<Guild>();
        foreach (var guildChunk in guildIds.Chunk(100))
            guilds.AddRange(await repository.Guild.GetGuildsByIdsAsync(guildChunk.ToList()));
        xmlData.Add(TransformGuilds(guilds));
    }

    private static async Task ProcessChannelsAsync(GrillBotRepository repository, List<string> channelIds, XContainer xmlData)
    {
        var channels = new List<GuildChannel?>();
        foreach (var channelChunk in channelIds.Chunk(100))
            channels.AddRange(await repository.Channel.GetChannelsByIdsAsync(channelChunk.ToList()));

        xmlData.Add(TransformChannels(channels));
    }

    private static async Task ProcessUsersAsync(GrillBotRepository repository, List<string> userIds, XContainer xmlData)
    {
        var users = new List<User?>();
        foreach (var userChunk in userIds.Chunk(100))
            users.AddRange(await repository.User.GetUsersByIdsAsync(userChunk.ToList()));

        xmlData.Add(TransformUsers(users));
    }

    private static string BuildReport(ArchivationResult result, string xmlSize, string zipSize)
    {
        var totalFilesSize = result.TotalFilesSize.Bytes().ToString();

        var builder = new StringBuilder()
            .AppendFormat("Items: {0}, Files: {1} ({2}), XmlSize: {3}, ZipSize: {4}", result.ItemsCount, result.Files.Count, totalFilesSize, xmlSize, zipSize)
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
