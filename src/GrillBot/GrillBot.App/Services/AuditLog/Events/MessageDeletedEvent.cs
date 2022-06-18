using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Extensions;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.FileStorage;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog.Events;

public class MessageDeletedEvent : AuditEventBase
{
    private Cacheable<IMessage, ulong> Message { get; }
    private Cacheable<IMessageChannel, ulong> Channel { get; }
    private MessageCacheManager MessageCache { get; }
    private FileStorageFactory FileStorageFactory { get; }

    public MessageDeletedEvent(AuditLogService auditLogService, AuditLogWriter auditLogWriter, Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel,
        MessageCacheManager messageCache, FileStorageFactory fileStorageFactory) : base(auditLogService, auditLogWriter)
    {
        Message = message;
        Channel = channel;
        MessageCache = messageCache;
        FileStorageFactory = fileStorageFactory;
    }

    public override Task<bool> CanProcessAsync() =>
        Task.FromResult(Channel.HasValue && Channel.Value is SocketTextChannel);

    public override async Task ProcessAsync()
    {
        var textChannel = (SocketTextChannel)Channel.Value;
        if ((Message.HasValue ? Message.Value : await MessageCache.GetAsync(Message.Id, null, true)) is not IUserMessage deletedMessage) return;

        var timeLimit = DateTime.UtcNow.AddMinutes(-1);
        var auditLog = (await textChannel.Guild.GetAuditLogsAsync(DiscordConfig.MaxAuditLogEntriesPerBatch, actionType: ActionType.MessageDeleted).FlattenAsync())
            .Where(o => o.CreatedAt.DateTime >= timeLimit)
            .FirstOrDefault(o =>
            {
                var data = (MessageDeleteAuditLogData)o.Data;
                return data.Target.Id == deletedMessage.Author.Id && data.ChannelId == textChannel.Id;
            });

        var data = new MessageDeletedData(deletedMessage);
        var removedBy = auditLog?.User ?? deletedMessage.Author;

        var attachments = await GetAndStoreAttachmentsAsync(deletedMessage);
        var item = new AuditLogDataWrapper(AuditLogItemType.MessageDeleted, data, textChannel.Guild, textChannel, removedBy, auditLog?.Id.ToString(), files: attachments);
        await AuditLogWriter.StoreAsync(item);
    }

    private async Task<List<AuditLogFileMeta>> GetAndStoreAttachmentsAsync(IUserMessage message)
    {
        if (message.Attachments.Count == 0) return null;

        var files = new List<AuditLogFileMeta>();
        var storage = FileStorageFactory.Create("Audit");

        foreach (var attachment in message.Attachments)
        {
            var content = await attachment.DownloadAsync();
            if (content == null) continue;

            var file = new AuditLogFileMeta()
            {
                Filename = attachment.Filename,
                Size = attachment.Size
            };

            var filenameWithoutExtension = file.FilenameWithoutExtension.Cut(100, true);
            var extension = file.Extension;
            file.Filename = string.Join("_", filenameWithoutExtension, attachment.Id.ToString(), message.Author.Id.ToString()) + extension;

            await storage.StoreFileAsync("DeletedAttachments", file.Filename, content);
            files.Add(file);
        }

        return files.Count == 0 ? null : files;
    }
}
