#pragma warning disable S1172 // Unused method parameters should be removed
using GrillBot.App.Extensions.Discord;
using GrillBot.Data.Extensions.Discord;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Services.AuditLog;

public partial class AuditLogService
{
    private static Task<bool> CanProcessMessageUpdated(Cacheable<IMessage, ulong> _, SocketMessage __, ISocketMessageChannel channel)
        => Task.FromResult(channel is SocketTextChannel);

    private async Task ProcessMessageUpdatedAsync(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel)
    {
        var textChannel = channel as SocketTextChannel;
        var oldMessage = before.HasValue ? before.Value : MessageCache.GetMessage(before.Id);
        if (oldMessage == null || after == null || !oldMessage.Author.IsUser() || oldMessage.Content == after.Content) return;
        var author = await DiscordClient.TryFindGuildUserAsync(textChannel.Guild.Id, oldMessage.Author.Id);
        if (author == null) return;

        var data = new MessageEditedData(oldMessage, after);
        var jsonData = JsonConvert.SerializeObject(data, JsonSerializerSettings);
        await StoreItemAsync(AuditLogItemType.MessageEdited, textChannel.Guild, textChannel, author, jsonData);
    }
}
