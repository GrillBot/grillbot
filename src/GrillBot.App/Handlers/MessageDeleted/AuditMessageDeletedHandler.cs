using Azure;
using Azure.Storage.Blobs.Models;
using GrillBot.App.Helpers;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.FileStorage;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.Extensions;
using GrillBot.Core.RabbitMQ.V2.Publisher;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Events.Create;

namespace GrillBot.App.Handlers.MessageDeleted;

public class AuditMessageDeletedHandler : IMessageDeletedEvent
{
    private IMessageCacheManager MessageCache { get; }
    private DownloadHelper DownloadHelper { get; }
    private BlobManagerFactoryHelper BlobManagerFactoryHelper { get; }

    private readonly IRabbitPublisher _rabbitPublisher;

    public AuditMessageDeletedHandler(IMessageCacheManager messageCache, DownloadHelper downloadHelper, BlobManagerFactoryHelper blobManagerFactoryHelper, IRabbitPublisher rabbitPublisher)
    {
        MessageCache = messageCache;
        DownloadHelper = downloadHelper;
        BlobManagerFactoryHelper = blobManagerFactoryHelper;
        _rabbitPublisher = rabbitPublisher;
    }

    public async Task ProcessAsync(Cacheable<IMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel)
    {
        if (!cachedChannel.HasValue || cachedChannel.Value is not ITextChannel textChannel) return;

        var message = cachedMessage.HasValue ? cachedMessage.Value : null;
        message ??= await MessageCache.GetAsync(cachedMessage.Id, null, true) as IUserMessage;
        if (message == null) return;

        var guildId = textChannel.Guild.Id.ToString();
        var channelId = textChannel.Id.ToString();

        var request = new LogRequest(LogType.MessageDeleted, DateTime.UtcNow, guildId, null, channelId)
        {
            Files = await GetAndStoreAttachmentsAsync(message),
            MessageDeleted = new MessageDeletedRequest
            {
                Content = message.Content,
                Embeds = ConvertEmbeds(message).ToList(),
                AuthorId = message.Author.Id.ToString(),
                MessageCreatedAt = message.CreatedAt.UtcDateTime
            }
        };

        await _rabbitPublisher.PublishAsync(new CreateItemsMessage(request));
    }

    private async Task<List<FileRequest>> GetAndStoreAttachmentsAsync(IMessage message)
    {
        var files = new List<FileRequest>();
        if (message.Attachments.Count == 0)
            return files;

        var manager = await BlobManagerFactoryHelper.CreateAsync(BlobConstants.AuditLogDeletedAttachments);

        foreach (var attachment in message.Attachments)
        {
            var content = await DownloadHelper.DownloadAsync(attachment);
            if (content is null) continue;

            var extension = Path.GetExtension(attachment.Filename);
            var filenameWithoutExtension = Path.GetFileNameWithoutExtension(attachment.Filename).Cut(100, true);
            var file = new FileRequest
            {
                Filename = string.Join("_", filenameWithoutExtension, attachment.Id.ToString(), message.Author.Id.ToString()) + extension,
                Extension = extension,
                Size = attachment.Size
            };

            files.Add(file);

            try
            {
                await manager.UploadAsync(file.Filename, content);
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.BlobAlreadyExists)
            {
                // Can ignore.
            }
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
                Fields = embed.Fields
                    .Where(o => !string.IsNullOrEmpty(o.Value))
                    .Select(o => new EmbedFieldBuilder().WithName(o.Name).WithValue(o.Value).WithIsInline(o.Inline))
                    .ToList()
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

        const StringSplitOptions splitFlags = StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries;

        var url = new Uri(video.Value.Url);
        var channels = url.Query[1..]
            .Split('&', splitFlags)
            .Where(o => o.StartsWith("channel="))
            .Select(o => o["channel=".Length..].Trim())
            .Where(o => !string.IsNullOrEmpty(o));

        return $"{string.Join(", ", channels)} {size}";
    }
}
