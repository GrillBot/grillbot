using System.IO.Compression;
using GrillBot.App.Helpers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Core.Services.RubbergodService;
using GrillBot.Core.Exceptions;
using GrillBot.Core.IO;

namespace GrillBot.App.Actions.Api.V1.Channel;

public class GetPinsWithAttachments : ApiAction
{
    private ChannelHelper ChannelHelper { get; }
    private ITextsManager Texts { get; }
    private IRubbergodServiceClient RubbergodService { get; }
    private DownloadHelper DownloadHelper { get; }

    public GetPinsWithAttachments(ApiRequestContext apiContext, ChannelHelper channelHelper, ITextsManager texts, IRubbergodServiceClient rubbergodService, DownloadHelper downloadHelper) :
        base(apiContext)
    {
        ChannelHelper = channelHelper;
        Texts = texts;
        RubbergodService = rubbergodService;
        DownloadHelper = downloadHelper;
    }

    public async Task<byte[]> ProcessAsync(ulong channelId)
    {
        var guild = await ChannelHelper.GetGuildFromChannelAsync(null, channelId);
        if (guild is null)
            throw new NotFoundException(Texts["ChannelModule/ChannelDetail/ChannelNotFound", ApiContext.Language]);

        var markdownContent = await RubbergodService.GetPinsAsync(guild.Id, channelId, true);

        TemporaryFile? archiveFile = null;
        ZipArchive? archive = null;

        try
        {
            (archiveFile, archive) = await CreateTemporaryZipAsync(markdownContent);
            await AppendAttachmentsAsync(guild, channelId, archive);
            archive.Dispose();

            return await File.ReadAllBytesAsync(archiveFile.Path);
        }
        finally
        {
            archive?.Dispose();
            archiveFile?.Dispose();
        }
    }

    private async Task AppendAttachmentsAsync(IGuild guild, ulong channelId, ZipArchive archive)
    {
        var rawData = Encoding.UTF8.GetString(await RubbergodService.GetPinsAsync(guild.Id, channelId, false));
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

                await using (var attachmentEntryStream = attachmentEntry.Open())
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
