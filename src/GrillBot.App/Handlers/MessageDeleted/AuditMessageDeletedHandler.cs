using GrillBot.App.Helpers;
using GrillBot.App.Managers;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Common.Services.FileService;
using GrillBot.Core.Extensions;
using GrillBot.Core.Managers.Performance;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using Microsoft.AspNetCore.StaticFiles;

namespace GrillBot.App.Handlers.MessageDeleted;

public class AuditMessageDeletedHandler : IMessageDeletedEvent
{
    private IMessageCacheManager MessageCache { get; }
    private AuditLogWriteManager AuditLogWriteManager { get; }
    private ICounterManager CounterManager { get; }
    private DownloadHelper DownloadHelper { get; }
    private IFileServiceClient FileServiceClient { get; }

    public AuditMessageDeletedHandler(IMessageCacheManager messageCache, AuditLogWriteManager auditLogWriteManager, ICounterManager counterManager, DownloadHelper downloadHelper,
        IFileServiceClient fileServiceClient)
    {
        MessageCache = messageCache;
        AuditLogWriteManager = auditLogWriteManager;
        CounterManager = counterManager;
        DownloadHelper = downloadHelper;
        FileServiceClient = fileServiceClient;
    }

    public async Task ProcessAsync(Cacheable<IMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel)
    {
        if (!cachedChannel.HasValue || cachedChannel.Value is not ITextChannel textChannel) return;

        var message = cachedMessage.HasValue ? cachedMessage.Value : null;
        message ??= (await MessageCache.GetAsync(cachedMessage.Id, null, true)) as IUserMessage;
        if (message == null) return;

        var auditLog = await FindAuditLogAsync(textChannel, message);
        var data = new MessageDeletedData(message);
        var removedBy = auditLog?.User ?? message.Author;
        var attachments = await GetAndStoreAttachmentsAsync(message);
        var item = new AuditLogDataWrapper(AuditLogItemType.MessageDeleted, data, textChannel.Guild, textChannel, removedBy, auditLog?.Id.ToString(), files: attachments);

        await AuditLogWriteManager.StoreAsync(item);
    }

    private async Task<IAuditLogEntry?> FindAuditLogAsync(IGuildChannel channel, IMessage message)
    {
        IReadOnlyCollection<IAuditLogEntry> auditLogs;
        using (CounterManager.Create("Discord.API.AuditLog"))
        {
            auditLogs = await channel.Guild.GetAuditLogsAsync(actionType: ActionType.MessageDeleted);
        }

        var timeLimit = DateTime.UtcNow.AddMinutes(-1);
        return auditLogs.FirstOrDefault(o =>
        {
            var data = (MessageDeleteAuditLogData)o.Data;
            return o.CreatedAt.DateTime >= timeLimit && data.Target.Id == message.Author.Id && data.ChannelId == message.Channel.Id;
        });
    }

    private async Task<List<AuditLogFileMeta>?> GetAndStoreAttachmentsAsync(IMessage message)
    {
        if (message.Attachments.Count == 0) return null;

        var files = new List<AuditLogFileMeta>();
        foreach (var attachment in message.Attachments)
        {
            var content = await DownloadHelper.DownloadAsync(attachment);
            if (content == null) continue;

            var file = new AuditLogFileMeta
            {
                Filename = attachment.Filename,
                Size = attachment.Size
            };

            var filenameWithoutExtension = file.FilenameWithoutExtension.Cut(100, true);
            var extension = file.Extension;
            file.Filename = string.Join("_", filenameWithoutExtension, attachment.Id.ToString(), message.Author.Id.ToString(), extension);

            var contentType = new FileExtensionContentTypeProvider().TryGetContentType(file.Filename, out var type) ? type : "application/octet-stream";
            await FileServiceClient.UploadFileAsync(file.Filename, content, contentType);

            files.Add(file);
        }

        return files;
    }
}
