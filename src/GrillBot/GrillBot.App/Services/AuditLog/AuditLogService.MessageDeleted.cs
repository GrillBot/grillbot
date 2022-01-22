#pragma warning disable RCS1163 // Unused parameter.
using GrillBot.App.Extensions.Discord;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog;

public partial class AuditLogService
{
    private Task<bool> CanProcessMessageDeletedAsync(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel)
        => Task.FromResult(channel.HasValue && channel.Value is SocketTextChannel);

    private async Task ProcessChannelDeletedAsync(Cacheable<IMessage, ulong> message, Cacheable<IMessageChannel, ulong> channel)
    {
        var textChannel = channel.Value as SocketTextChannel;
        if ((message.HasValue ? message.Value : MessageCache.GetMessage(message.Id, true)) is not IUserMessage deletedMessage) return;
        var timeLimit = DateTime.UtcNow.AddMinutes(-1);
        var auditLog = (await textChannel.Guild.GetAuditLogsAsync(5, actionType: ActionType.MessageDeleted).FlattenAsync())
            .Where(o => o.CreatedAt.DateTime >= timeLimit)
            .FirstOrDefault(o =>
            {
                var data = (MessageDeleteAuditLogData)o.Data;
                return data.Target.Id == deletedMessage.Author.Id && data.ChannelId == channel.Id;
            });

        var data = new MessageDeletedData(deletedMessage);
        var jsonData = JsonConvert.SerializeObject(data, JsonSerializerSettings);
        var removedBy = auditLog?.User ?? deletedMessage.Author;

        var attachments = await GetAndStoreAttachmentsAsync(deletedMessage);
        await StoreItemAsync(AuditLogItemType.MessageDeleted, textChannel.Guild, textChannel, removedBy, jsonData, auditLog?.Id, null, attachments);
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

            var filenameWithoutExtension = file.FilenameWithoutExtension;
            var extension = file.Extension;
            file.Filename = string.Join("_", new[] {
                filenameWithoutExtension,
                attachment.Id.ToString(),
                message.Author.Id.ToString()
            }) + extension;

            await storage.StoreFileAsync("DeletedAttachments", file.Filename, content);
            files.Add(file);
        }

        return files.Count == 0 ? null : files;
    }
}
