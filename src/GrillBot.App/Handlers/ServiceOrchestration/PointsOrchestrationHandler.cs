using GrillBot.App.Managers.Points;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;
using PointsService.Models;
using PointsService.Models.Channels;
using PointsService.Models.Events;

namespace GrillBot.App.Handlers.ServiceOrchestration;

public class PointsOrchestrationHandler(
    IMessageCacheManager _messageCache,
    PointsManager _pointsManager
) : IChannelDestroyedEvent, IThreadDeletedEvent, IMessageDeletedEvent, IReactionAddedEvent, IReactionRemovedEvent
{

    // ChannelDestroyed
    public Task ProcessAsync(IChannel channel)
    {
        if (channel is IThreadChannel || channel is not IGuildChannel guildChannel)
            return Task.CompletedTask;

        var syncItem = new ChannelSyncItem
        {
            Id = channel.Id.ToString(),
            IsDeleted = true
        };

        return _pointsManager.PushSynchronizationAsync(guildChannel.Guild, [], [syncItem]);
    }

    // ThreadDeleted
    public Task ProcessAsync(IThreadChannel? cachedThread, ulong threadId)
    {
        if (cachedThread is null)
            return Task.CompletedTask;

        var syncItem = new ChannelSyncItem
        {
            IsDeleted = true,
            Id = threadId.ToString()
        };

        return _pointsManager.PushSynchronizationAsync(cachedThread.Guild, [], [syncItem]);
    }

    // MessageDeleted
    public Task ProcessAsync(Cacheable<IMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel)
    {
        if (!cachedChannel.HasValue || cachedChannel.Value is not IGuildChannel guildChannel)
            return Task.CompletedTask;

        var payload = new DeleteTransactionsPayload(guildChannel.GuildId.ToString(), cachedMessage.Id.ToString());
        return _pointsManager.PushPayloadAsync(payload);
    }

    // ReactionAdded
    public async Task ProcessAsync(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel, SocketReaction reaction)
    {
        if (!cachedChannel.HasValue || cachedChannel.Value is not ITextChannel textChannel) return;
        if (reaction.Emote is not Discord.Emote || !textChannel.Guild.Emotes.Any(x => x.IsEqual(reaction.Emote))) return;

        var reactionUser = reaction.User.IsSpecified ? reaction.User.GetValueOrDefault() : await textChannel.Guild.GetUserAsync(reaction.UserId);
        if (reactionUser is null) return;

        var message = cachedMessage.HasValue ? cachedMessage.Value : await _messageCache.GetAsync(cachedMessage.Id, textChannel);
        if (message is null) return;

        if (!_pointsManager.CanIncrementPoints(message, reactionUser))
            return;

        var messageInfo = new MessageInfo
        {
            AuthorId = message.Author.Id.ToString(),
            Id = message.Id.ToString(),
            ContentLength = message.Content.Length,
            MessageType = message.Type
        };

        var reactionInfo = new ReactionInfo
        {
            Emote = reaction.Emote.ToString()!,
            IsBurst = reaction.IsBurst,
            UserId = reaction.UserId.ToString()
        };

        var payload = new CreateTransactionPayload(textChannel.GuildId.ToString(), DateTime.UtcNow, textChannel.Id.ToString(), messageInfo, reactionInfo);
        await _pointsManager.PushPayloadAsync(payload);
    }

    // ReactionRemoved
    Task IReactionRemovedEvent.ProcessAsync(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel, SocketReaction reaction)
    {
        if (!cachedChannel.HasValue || cachedChannel.Value is not ITextChannel textChannel)
            return Task.CompletedTask;
        if (reaction.Emote is not Discord.Emote || !textChannel.Guild.Emotes.Any(x => x.IsEqual(reaction.Emote)))
            return Task.CompletedTask;

        var reactionId = new ReactionInfo
        {
            Emote = reaction.Emote.ToString()!,
            IsBurst = reaction.IsBurst,
            UserId = reaction.UserId.ToString()
        }.GetReactionId();

        var payload = new DeleteTransactionsPayload(textChannel.GuildId.ToString(), cachedMessage.Id.ToString(), reactionId);
        return _pointsManager.PushPayloadAsync(payload);
    }
}
