using GrillBot.App.Helpers;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.Services.PointsService.Models;
using GrillBot.Core.Services.PointsService.Models.Events;

namespace GrillBot.App.Handlers.ServiceOrchestration;

public class PointsOrchestrationHandler : IChannelDestroyedEvent, IThreadDeletedEvent, IMessageReceivedEvent, IMessageDeletedEvent, IReactionAddedEvent, IReactionRemovedEvent
{
    private PointsHelper PointsHelper { get; }
    private IMessageCacheManager MessageCache { get; }

    public PointsOrchestrationHandler(PointsHelper pointsHelper, IMessageCacheManager messageCache)
    {
        PointsHelper = pointsHelper;
        MessageCache = messageCache;
    }

    // ChannelDestroyed
    public async Task ProcessAsync(IChannel channel)
    {
        if (channel is not IThreadChannel && channel is IGuildChannel guildChannel)
            await PointsHelper.PushSynchronizationAsync(guildChannel);
    }

    // ThreadDeleted
    public async Task ProcessAsync(IThreadChannel? cachedThread, ulong threadId)
    {
        if (cachedThread is not null)
            await PointsHelper.PushSynchronizationAsync(cachedThread);
    }

    // MessageReceived
    public async Task ProcessAsync(IMessage message)
    {
        if (!await PointsHelper.CanIncrementPointsAsync(message))
            return;

        var channel = (IGuildChannel)message.Channel;
        await PointsHelper.PushSynchronizationAsync(channel.Guild, new[] { message.Author }, new[] { channel });

        var messageInfo = new MessageInfo
        {
            AuthorId = message.Author.Id.ToString(),
            Id = message.Id.ToString(),
            ContentLength = message.Content.Length,
            MessageType = message.Type
        };

        var payload = new CreateTransactionPayload(channel.GuildId.ToString(), message.CreatedAt.UtcDateTime, channel.Id.ToString(), messageInfo);
        await PointsHelper.PushPayloadAsync(payload);
    }

    // MessageDeleted
    public async Task ProcessAsync(Cacheable<IMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel)
    {
        if (!cachedChannel.HasValue || cachedChannel.Value is not IGuildChannel guildChannel)
            return;

        var payload = new DeleteTransactionsPayload(guildChannel.GuildId.ToString(), cachedMessage.Id.ToString());
        await PointsHelper.PushPayloadAsync(payload);
    }

    // ReactionAdded
    public async Task ProcessAsync(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel, SocketReaction reaction)
    {
        if (!cachedChannel.HasValue || cachedChannel.Value is not ITextChannel textChannel) return;
        if (reaction.Emote is not Emote || !textChannel.Guild.Emotes.Any(x => x.IsEqual(reaction.Emote))) return;

        var reactionUser = reaction.User.IsSpecified ? reaction.User.GetValueOrDefault() : await textChannel.Guild.GetUserAsync(reaction.UserId);
        if (reactionUser is null) return;

        var message = cachedMessage.HasValue ? cachedMessage.Value : await MessageCache.GetAsync(cachedMessage.Id, textChannel);
        if (message is null) return;

        if (!await PointsHelper.CanIncrementPointsAsync(message, reactionUser))
            return;

        await PointsHelper.PushSynchronizationAsync(textChannel.Guild, new[] { message.Author, reactionUser }, new[] { textChannel });

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
        await PointsHelper.PushPayloadAsync(payload);
    }

    // ReactionRemoved
    async Task IReactionRemovedEvent.ProcessAsync(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel, SocketReaction reaction)
    {
        if (!cachedChannel.HasValue || cachedChannel.Value is not ITextChannel textChannel) return;
        if (reaction.Emote is not Emote || !textChannel.Guild.Emotes.Any(x => x.IsEqual(reaction.Emote))) return;

        var reactionId = new ReactionInfo
        {
            Emote = reaction.Emote.ToString()!,
            IsBurst = reaction.IsBurst,
            UserId = reaction.UserId.ToString()
        }.GetReactionId();

        var payload = new DeleteTransactionsPayload(textChannel.GuildId.ToString(), cachedMessage.Id.ToString(), reactionId);
        await PointsHelper.PushPayloadAsync(payload);
    }
}
