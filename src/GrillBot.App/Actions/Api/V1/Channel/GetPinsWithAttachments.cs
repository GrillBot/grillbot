using System.IO.Compression;
using GrillBot.App.Helpers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using RubbergodService;
using GrillBot.Core.Exceptions;
using GrillBot.Core.IO;
using GrillBot.Core.Infrastructure.Actions;
using Microsoft.AspNetCore.Mvc;
using GrillBot.Core.Services.Common.Executor;

namespace GrillBot.App.Actions.Api.V1.Channel;

public class GetPinsWithAttachments(
    ApiRequestContext apiContext,
    ChannelHelper _channelHelper,
    ITextsManager _texts,
    IServiceClientExecutor<IRubbergodServiceClient> _rubbergodService,
    DownloadHelper _downloadHelper
) : ApiAction(apiContext)
{
    public override async Task<ApiResult> ProcessAsync()
    {
        var channelId = GetParameter<ulong>(0);

        var guild = await _channelHelper.GetGuildFromChannelAsync(null, channelId, CancellationToken)
            ?? throw new NotFoundException(_texts["ChannelModule/ChannelDetail/ChannelNotFound", ApiContext.Language]);

        using var response = await _rubbergodService.ExecuteRequestAsync(
            (c, ctx) => c.GetPinsAsync(guild.Id, channelId, true, ctx.CancellationToken),
            CancellationToken
        );

        var markdownContent = await response.ReadAsByteArrayAsync(CancellationToken);

        TemporaryFile? archiveFile = null;

        try
        {
            ZipArchive? archive;
            (archiveFile, archive) = await CreateTemporaryZipAsync(markdownContent);
            await AppendAttachmentsAsync(guild, channelId, archive);
            archive.Dispose();

            var result = await archiveFile.ReadAllBytesAsync(CancellationToken);
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
        using var response = await _rubbergodService.ExecuteRequestAsync(
            (c, ctx) => c.GetPinsAsync(guild.Id, channelId, false, ctx.CancellationToken),
            CancellationToken
        );

        var bytes = await response.ReadAsByteArrayAsync(CancellationToken);
        var rawData = Encoding.UTF8.GetString(bytes);
        var json = JObject.Parse(rawData);

        foreach (var pin in (JArray)json["pins"]!)
        {
            foreach (var attachment in (JArray)pin["attachments"]!)
            {
                var url = attachment["url"]!.Value<string>();
                if (string.IsNullOrEmpty(url))
                    continue;

                var content = await _downloadHelper.DownloadFileAsync(url, CancellationToken);
                if (content is null)
                    continue;

                var filename = attachment["name"]!.Value<string>()!;
                var attachmentEntry = archive.CreateEntry(filename);

                await using var attachmentEntryStream = attachmentEntry.Open();
                await attachmentEntryStream.WriteAsync(content, CancellationToken);
            }
        }
    }

    private async Task<(TemporaryFile, ZipArchive)> CreateTemporaryZipAsync(byte[] markdown)
    {
        var archiveFile = new TemporaryFile("zip");
        var archive = ZipFile.Open(archiveFile.Path, ZipArchiveMode.Create);

        var markdownEntry = archive.CreateEntry("channel.md");
        await using (var markdownEntryStream = markdownEntry.Open())
            await markdownEntryStream.WriteAsync(markdown, CancellationToken);

        return (archiveFile, archive);
    }
}
