using GrillBot.App.Extensions.Discord;
using GrillBot.Data.Extensions.Discord;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog.Events;

public class MessageEditedEvent : AuditEventBase
{
    private Cacheable<IMessage, ulong> Before { get; }
    private SocketMessage After { get; }
    private ISocketMessageChannel Channel { get; }
    private MessageCache.MessageCache MessageCache { get; }
    private DiscordSocketClient DiscordClient { get; }

    private SocketTextChannel TextChannel => Channel as SocketTextChannel;

    public MessageEditedEvent(AuditLogService auditLogService, Cacheable<IMessage, ulong> before, SocketMessage after,
        ISocketMessageChannel channel, MessageCache.MessageCache messageCache, DiscordSocketClient discordClient) : base(auditLogService)
    {
        Before = before;
        After = after;
        Channel = channel;
        MessageCache = messageCache;
        DiscordClient = discordClient;
    }

    public override Task<bool> CanProcessAsync()
        => Task.FromResult(TextChannel != null);

    public override async Task ProcessAsync()
    {
        var textChannel = TextChannel;
        var oldMessage = Before.HasValue ? Before.Value : MessageCache.GetMessage(Before.Id);
        if (oldMessage == null || After == null || !oldMessage.Author.IsUser() || oldMessage.Content == After.Content) return;
        var author = await DiscordClient.TryFindGuildUserAsync(textChannel.Guild.Id, oldMessage.Author.Id);
        if (author == null) return;

        var data = new MessageEditedData(oldMessage, After);
        var jsonData = JsonConvert.SerializeObject(data, AuditLogService.JsonSerializerSettings);
        await AuditLogService.StoreItemAsync(AuditLogItemType.MessageEdited, textChannel.Guild, textChannel, author, jsonData);
    }
}
