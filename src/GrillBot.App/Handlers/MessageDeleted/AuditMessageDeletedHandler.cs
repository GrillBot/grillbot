using AuditLogService.Models.Request;
using GrillBot.App.Helpers;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Common.Services.AuditLog;
using GrillBot.Common.Services.AuditLog.Enums;
using GrillBot.Common.Services.AuditLog.Models;
using GrillBot.Common.Services.FileService;
using GrillBot.Core.Extensions;
using Microsoft.AspNetCore.StaticFiles;

namespace GrillBot.App.Handlers.MessageDeleted;

public class AuditMessageDeletedHandler : AuditLogServiceHandler, IMessageDeletedEvent
{
    private IMessageCacheManager MessageCache { get; }
    private DownloadHelper DownloadHelper { get; }
    private IFileServiceClient FileServiceClient { get; }

    public AuditMessageDeletedHandler(IMessageCacheManager messageCache, DownloadHelper downloadHelper, IFileServiceClient fileServiceClient, IAuditLogServiceClient auditLogServiceClient) :
        base(auditLogServiceClient)
    {
        MessageCache = messageCache;
        DownloadHelper = downloadHelper;
        FileServiceClient = fileServiceClient;
    }

    public async Task ProcessAsync(Cacheable<IMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel)
    {
        if (!cachedChannel.HasValue || cachedChannel.Value is not ITextChannel textChannel) return;

        var message = cachedMessage.HasValue ? cachedMessage.Value : null;
        message ??= await MessageCache.GetAsync(cachedMessage.Id, null, true) as IUserMessage;
        if (message == null) return;

        var request = CreateRequest(LogType.MessageDeleted, textChannel.Guild, textChannel);
        request.Files = await GetAndStoreAttachmentsAsync(message);
        request.MessageDeleted = new MessageDeletedRequest
        {
            Content = message.Content,
            Embeds = ConvertEmbeds(message).ToList(),
            AuthorId = message.Author.Id.ToString(),
            MessageCreatedAt = message.CreatedAt.UtcDateTime
        };

        await SendRequestAsync(request);
    }

    private async Task<List<FileRequest>> GetAndStoreAttachmentsAsync(IMessage message)
    {
        var files = new List<FileRequest>();
        if (message.Attachments.Count == 0)
            return files;

        var contentTypeProvider = new FileExtensionContentTypeProvider();
        foreach (var attachment in message.Attachments)
        {
            var content = await DownloadHelper.DownloadAsync(attachment);
            if (content == null) continue;

            var extension = Path.GetExtension(attachment.Filename);
            var filenameWithoutExtension = Path.GetFileNameWithoutExtension(attachment.Filename).Cut(100, true);
            var file = new FileRequest
            {
                Filename = string.Join("_", filenameWithoutExtension, attachment.Id.ToString(), message.Author.Id.ToString()) + extension,
                Extension = extension,
                Size = attachment.Size
            };

            var contentType = contentTypeProvider.TryGetContentType(file.Filename, out var type) ? type : "application/octet-stream";
            await FileServiceClient.UploadFileAsync(file.Filename, content, contentType);

            files.Add(file);
        }

        return files;
    }

    private static IEnumerable<EmbedRequest> ConvertEmbeds(IMessage message)
    {
        foreach (var embed in message.Embeds)
        {
            var providerName = embed.Provider?.Name;
            var request = new EmbedRequest
            {
                Title = embed.Title,
                Type = embed.Type.ToString(),
                AuthorName = embed.Author?.Name,
                ContainsFooter = embed.Footer is not null,
                ProviderName = providerName,
                VideoInfo = ParseVideoInfo(embed.Video, providerName),
                Fields = embed.Fields.Select(o => new EmbedFieldBuilder().WithName(o.Name).WithValue(o.Value).WithIsInline(o.Inline)).ToList()
            };

            if (embed.Image is not null)
                request.ImageInfo = $"{embed.Image.Value.Url} ({embed.Image.Value.Width}x{embed.Image.Value.Height})";
            if (embed.Thumbnail is not null)
                request.ThumbnailInfo = $"{embed.Thumbnail!.Value.Url} ({embed.Thumbnail.Value.Width}x{embed.Thumbnail.Value.Height})";

            yield return request;
        }
    }

    private static string? ParseVideoInfo(EmbedVideo? video, string? providerName)
    {
        if (video is null)
            return null;

        var size = $"({video.Value.Width}x{video.Value.Height})";

        if (providerName != "Twitch")
            return $"{video.Value.Url} {size}";

        var url = new Uri(video.Value.Url);
        var queryFields = url.Query[1..].Split('&').Select(o => o.Split('=')).ToDictionary(o => o[0], o => o[1]);
        return $"{queryFields["channel"]} {size}";
    }
}
