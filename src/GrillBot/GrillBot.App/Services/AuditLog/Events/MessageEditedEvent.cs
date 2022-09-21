using GrillBot.Cache.Services.Managers;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Services.AuditLog.Events;

public class MessageEditedEvent : AuditEventBase
{
    private Cacheable<IMessage, ulong> Before { get; }
    private SocketMessage After { get; }
    private ISocketMessageChannel Channel { get; }
    private IMessageCacheManager MessageCache { get; }
    private IDiscordClient DiscordClient { get; }

    private SocketTextChannel TextChannel => Channel as SocketTextChannel;

    public MessageEditedEvent(AuditLogService auditLogService, AuditLogWriter auditLogWriter, IServiceProvider serviceProvider,
        Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel) : base(auditLogService, auditLogWriter)
    {
        Before = before;
        After = after;
        Channel = channel;
        MessageCache = serviceProvider.GetRequiredService<IMessageCacheManager>();
        DiscordClient = serviceProvider.GetRequiredService<IDiscordClient>();
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
