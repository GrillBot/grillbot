using System.Diagnostics.CodeAnalysis;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Common.Services.AuditLog;
using GrillBot.Common.Services.AuditLog.Enums;
using GrillBot.Common.Services.AuditLog.Models;

namespace GrillBot.App.Handlers.MessageUpdated;

public class AuditMessageUpdatedHandler : AuditLogServiceHandler, IMessageUpdatedEvent
{
    private IMessageCacheManager MessageCacheManager { get; }

    public AuditMessageUpdatedHandler(IMessageCacheManager messageCacheManager, IAuditLogServiceClient client) : base(client)
    {
        MessageCacheManager = messageCacheManager;
    }

    public async Task ProcessAsync(Cacheable<IMessage, ulong> before, IMessage after, IMessageChannel channel)
    {
        var oldMessage = before.HasValue ? before.Value : null;
        oldMessage ??= await MessageCacheManager.GetAsync(before.Id, null);
        if (!Init(channel, oldMessage, after, out var textChannel)) return;

        var author = after.Author as IGuildUser ?? await textChannel.Guild.GetUserAsync(after.Author.Id);
        var request = CreateRequest(LogType.MessageEdited, textChannel.Guild, textChannel, author);
        request.MessageEdited = new MessageEditedRequest
        {
            ContentAfter = after.Content,
            ContentBefore = oldMessage!.Content,
            JumpUrl = after.GetJumpUrl()
        };

        await SendRequestAsync(request);
    }

    private static bool Init(IMessageChannel channel, IMessage? before, IMessage after, [MaybeNullWhen(false)] out ITextChannel textChannel)
    {
        textChannel = channel as ITextChannel;

        return textChannel is not null && before is not null && before.Author.IsUser() && !string.IsNullOrEmpty(after.Content) && before.Content != after.Content &&
               before.Type != MessageType.ApplicationCommand;
    }
}
