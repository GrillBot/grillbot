using GrillBot.App.Services.AuditLog;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Handlers.MessageUpdated;

public class AuditMessageUpdatedHandler : IMessageUpdatedEvent
{
    private IMessageCacheManager MessageCacheManager { get; }
    private AuditLogWriter AuditLogWriter { get; }

    public AuditMessageUpdatedHandler(IMessageCacheManager messageCacheManager, AuditLogWriter auditLogWriter)
    {
        MessageCacheManager = messageCacheManager;
        AuditLogWriter = auditLogWriter;
    }

    public async Task ProcessAsync(Cacheable<IMessage, ulong> before, IMessage after, IMessageChannel channel)
    {
        var oldMessage = before.HasValue ? before.Value : null;
        oldMessage ??= await MessageCacheManager.GetAsync(before.Id, null);
        if (!Init(channel, oldMessage, after, out var textChannel)) return;

        var author = after.Author as IGuildUser ?? await textChannel.Guild.GetUserAsync(after.Author.Id);
        var data = new MessageEditedData(oldMessage, after);
        var item = new AuditLogDataWrapper(AuditLogItemType.MessageEdited, data, textChannel.Guild, textChannel, author);

        await AuditLogWriter.StoreAsync(item);
    }

    private static bool Init(IMessageChannel channel, IMessage before, IMessage after, out ITextChannel textChannel)
    {
        textChannel = channel as ITextChannel;

        return textChannel != null && before != null && before.Author.IsUser() && !string.IsNullOrEmpty(after.Content) && before.Content != after.Content &&
               before.Type != MessageType.ApplicationCommand;
    }
}
