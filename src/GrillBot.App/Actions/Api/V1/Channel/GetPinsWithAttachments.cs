using System.IO.Compression;
using GrillBot.App.Helpers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Common.Models;
using GrillBot.Common.Services.RubbergodService;
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
        var attachments = await GetAttachmentsAsync(guild, channelId);

        using var archive = await CreateArchiveAsync(markdownContent, attachments);
        return await File.ReadAllBytesAsync(archive.Path);
    }

    private async Task<List<(string filename, byte[] attachment)>> GetAttachmentsAsync(IGuild guild, ulong channelId)
    {
        var rawData = Encoding.UTF8.GetString(await RubbergodService.GetPinsAsync(guild.Id, channelId, false));
        var json = JObject.Parse(rawData);
        var result = new List<(string, byte[])>();

        foreach (var pin in (JArray)json["pins"]!)
        {
            foreach (var attachment in (JArray)pin["attachments"]!)
            {
                var filename = attachment["name"]!.Value<string>()!;
                var url = attachment["url"]!.Value<string>();
                if (string.IsNullOrEmpty(url))
                    continue;

                var content = await DownloadHelper.DownloadFileAsync(url);
                if (content is not null)
                    result.Add((filename, content));
            }
        }

        return result;
    }

    private static async Task<TemporaryFile> CreateArchiveAsync(byte[] markdown, List<(string filename, byte[] attachment)> attachments)
    {
        var archiveFile = new TemporaryFile("zip");
        using var archive = ZipFile.Open(archiveFile.Path, ZipArchiveMode.Create);

        var markdownEntry = archive.CreateEntry("channel.md");
        await using (var markdownEntryStream = markdownEntry.Open())
            await markdownEntryStream.WriteAsync(markdown);

        foreach (var attachment in attachments)
        {
            var entry = archive.CreateEntry(attachment.filename);
            await using var stream = entry.Open();
            await stream.WriteAsync(attachment.attachment);
        }

        return archiveFile;
    }
}
