using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog.Events;

public class MessageEditedEvent : AuditEventBase
{
    private Cacheable<IMessage, ulong> Before { get; }
    private SocketMessage After { get; }
    private ISocketMessageChannel Channel { get; }
    private MessageCacheManager MessageCache { get; }
    private IDiscordClient DiscordClient { get; }

    private SocketTextChannel TextChannel => Channel as SocketTextChannel;

    public MessageEditedEvent(AuditLogService auditLogService, AuditLogWriter auditLogWriter, Cacheable<IMessage, ulong> before, SocketMessage after,
        ISocketMessageChannel channel, MessageCacheManager messageCache, IDiscordClient discordClient) : base(auditLogService, auditLogWriter)
    {
        Before = before;
        After = after;
        Channel = channel;
        MessageCache = messageCache;
        DiscordClient = discordClient;
    }

    public override async Task<bool> CanProcessAsync()
    {
        var oldMessage = await GetOldMessageAsync();

        return TextChannel != null && oldMessage?.Author.IsUser() == true &&
               !string.IsNullOrEmpty(After?.Content) && oldMessage.Content != After.Content &&
               oldMessage.Type != MessageType.ApplicationCommand;
    }

    public override async Task ProcessAsync()
    {
        var textChannel = TextChannel;
        var oldMessage = await GetOldMessageAsync();
        var author = await DiscordClient.TryFindGuildUserAsync(textChannel.Guild.Id, oldMessage.Author.Id);
        if (author == null) return;

        var data = new MessageEditedData(oldMessage, After);
        var item = new AuditLogDataWrapper(AuditLogItemType.MessageEdited, data, textChannel.Guild, textChannel, author);
        await AuditLogWriter.StoreAsync(item);
    }

    private async Task<IMessage> GetOldMessageAsync()
        => Before.HasValue ? Before.Value : await MessageCache.GetAsync(Before.Id, null);
}
