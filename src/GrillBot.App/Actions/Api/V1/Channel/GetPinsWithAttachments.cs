using System.IO.Compression;
using GrillBot.App.Helpers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Services.RubbergodService;
using GrillBot.Core.Exceptions;
using GrillBot.Core.IO;
using GrillBot.Core.Infrastructure.Actions;
using Microsoft.AspNetCore.Mvc;
using GrillBot.Core.Services.Common.Executor;

namespace GrillBot.App.Actions.Api.V1.Channel;

public class GetPinsWithAttachments : ApiAction
{
    private ChannelHelper ChannelHelper { get; }
    private ITextsManager Texts { get; }
    private DownloadHelper DownloadHelper { get; }

    private readonly IServiceClientExecutor<IRubbergodServiceClient> _rubbergodServiceClient;

    public GetPinsWithAttachments(ApiRequestContext apiContext, ChannelHelper channelHelper, ITextsManager texts,
        IServiceClientExecutor<IRubbergodServiceClient> rubbergodService, DownloadHelper downloadHelper) : base(apiContext)
    {
        ChannelHelper = channelHelper;
        Texts = texts;
        _rubbergodServiceClient = rubbergodService;
        DownloadHelper = downloadHelper;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var channelId = (ulong)Parameters[0]!;

        var guild = await ChannelHelper.GetGuildFromChannelAsync(null, channelId)
            ?? throw new NotFoundException(Texts["ChannelModule/ChannelDetail/ChannelNotFound", ApiContext.Language]);

        var markdownContent = await _rubbergodServiceClient.ExecuteRequestAsync((c, cancellationToken) => c.GetPinsAsync(guild.Id, channelId, true, cancellationToken));

        TemporaryFile? archiveFile = null;

        try
        {
            ZipArchive? archive;
            (archiveFile, archive) = await CreateTemporaryZipAsync(markdownContent);
            await AppendAttachmentsAsync(guild, channelId, archive);
            archive.Dispose();

            var result = await archiveFile.ReadAllBytesAsync();
            var apiResult = new FileContentResult(result, "application/zip");

            return ApiResult.Ok(apiResult);
        }
        finally
        {
            archiveFile?.Dispose();
        }
    }

    private async Task AppendAttachmentsAsync(IGuild guild, ulong channelId, ZipArchive archive)
    {
        var bytes = await _rubbergodServiceClient.ExecuteRequestAsync((c, cancellationToken) => c.GetPinsAsync(guild.Id, channelId, false, cancellationToken));
        var rawData = Encoding.UTF8.GetString(bytes);
        var json = JObject.Parse(rawData);

        foreach (var pin in (JArray)json["pins"]!)
        {
            foreach (var attachment in (JArray)pin["attachments"]!)
            {
                var url = attachment["url"]!.Value<string>();
                if (string.IsNullOrEmpty(url))
                    continue;

                var content = await DownloadHelper.DownloadFileAsync(url);
                if (content is null)
                    continue;

                var filename = attachment["name"]!.Value<string>()!;
                var attachmentEntry = archive.CreateEntry(filename);

                await using var attachmentEntryStream = attachmentEntry.Open();
                await attachmentEntryStream.WriteAsync(content);
            }
        }
    }

    private static async Task<(TemporaryFile, ZipArchive)> CreateTemporaryZipAsync(byte[] markdown)
    {
        var archiveFile = new TemporaryFile("zip");
        var archive = ZipFile.Open(archiveFile.Path, ZipArchiveMode.Create);

        var markdownEntry = archive.CreateEntry("channel.md");
        await using (var markdownEntryStream = markdownEntry.Open())
            await markdownEntryStream.WriteAsync(markdown);

        return (archiveFile, archive);
    }
}
