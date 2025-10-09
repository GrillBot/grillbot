using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.Extensions;
using GrillBot.Core.RabbitMQ.V2.Publisher;
using GrillBot.Core.Services.Common.Executor;
using Emote;
using Emote.Models.Events;
using Emote.Models.Events.Guild;
using Microsoft.Extensions.Logging;

namespace GrillBot.App.Handlers.ServiceOrchestration;

public partial class EmoteOrchestrationHandler(
    IRabbitPublisher _rabbitPublisher,
    IMessageCacheManager _messageCache,
    IDiscordClient _discordClient,
    IServiceClientExecutor<IEmoteServiceClient> _emoteService,
    ILogger<EmoteOrchestrationHandler> _logger
) :
    IMessageDeletedEvent,
    IMessageReceivedEvent,
    IReactionAddedEvent,
    IReactionRemovedEvent,
    IReadyEvent,
    IGuildAvailableEvent,
    IGuildUpdatedEvent,
    IChannelDestroyedEvent
{
    // MessageReceived
    public async Task ProcessAsync(IMessage message)
    {
        if (!message.TryLoadMessage(out var userMessage)) return; // Ignore messages from bots.
        if (string.IsNullOrEmpty(userMessage?.Content)) return; // Ignore empty messages.
        if (message.IsCommand(_discordClient.CurrentUser)) return; // Ignore commands.
        if (message.Channel is not ITextChannel textChannel) return; // Ignore DMs
        if (message.Author is not IGuildUser guildUser) return; // Ignore non guild users.

        var guildId = textChannel.GuildId.ToString();
        var userId = guildUser.Id.ToString();
        var createdAt = DateTime.UtcNow;
        var payloads = message.GetEmotesFromMessage()
            .Select(e => new EmoteEventPayload(guildId, userId, e.ToString(), createdAt, true))
            .ToList();

        if (payloads.Count > 0)
            await _rabbitPublisher.PublishAsync(payloads);
    }

    // ReactionAdded
    public async Task ProcessAsync(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel, SocketReaction reaction)
    {
        if (!cachedChannel.HasValue || cachedChannel.Value is not ITextChannel textChannel) return;
        if (reaction.Emote is not Discord.Emote emote) return;

        var message = cachedMessage.HasValue ? cachedMessage.Value : null;
        message ??= await _messageCache.GetAsync(cachedMessage.Id, null, true) as IUserMessage;
        if (message is not { Author: IGuildUser author } || author.Id == reaction.UserId) return;

        var reactionUser = reaction.User.IsSpecified ? reaction.User.Value as IGuildUser : null;
        reactionUser ??= await textChannel.Guild.GetUserAsync(reaction.UserId);
        if (reactionUser is not IGuildUser reactionGuildUser) return;

        var guildId = reactionGuildUser.GuildId.ToString();
        var userId = reactionGuildUser.Id.ToString();
        var emoteId = emote.ToString();

        await _rabbitPublisher.PublishAsync(new EmoteEventPayload(guildId, userId, emoteId, DateTime.UtcNow, true));
    }

    // ReactionRemoved
    async Task IReactionRemovedEvent.ProcessAsync(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel, SocketReaction reaction)
    {
        if (!cachedChannel.HasValue || cachedChannel.Value is not ITextChannel textChannel) return;
        if (reaction.Emote is not Discord.Emote emote) return;

        var message = cachedMessage.HasValue ? cachedMessage.Value : null;
        message ??= await _messageCache.GetAsync(cachedMessage.Id, null, true) as IUserMessage;
        if (message is not { Author: IGuildUser author } || author.Id == reaction.UserId) return;

        var reactionUser = reaction.User.IsSpecified ? reaction.User.Value as IGuildUser : null;
        reactionUser ??= await textChannel.Guild.GetUserAsync(reaction.UserId);
        if (reactionUser is not IGuildUser reactionGuildUser) return;

        var guildId = reactionGuildUser.GuildId.ToString();
        var userId = reactionGuildUser.Id.ToString();
        var emoteId = emote.ToString();

        await _rabbitPublisher.PublishAsync(new EmoteEventPayload(guildId, userId, emoteId, DateTime.UtcNow, false));
    }

    // GuildAvailable
    public async Task ProcessAsync(IGuild guild)
    {
        var emotes = guild.Emotes.ToList();
        await _rabbitPublisher.PublishAsync(new SynchronizeEmotesPayload(guild.Id.ToString(), emotes));
    }

    // GuildUpdated
    public async Task ProcessAsync(IGuild before, IGuild after)
    {
        if (before.Emotes.Select(o => o.ToString()).IsSequenceEqual(after.Emotes.Select(o => o.ToString()))) return;

        var emotesAfter = after.Emotes.ToList();
        await _rabbitPublisher.PublishAsync(new SynchronizeEmotesPayload(after.Id.ToString(), emotesAfter));
    }

    // Channel destroyed
    public Task ProcessAsync(IChannel channel)
    {
        return channel is not IGuildChannel guildChannel
            ? Task.CompletedTask
            : _rabbitPublisher.PublishAsync(GuildChannelDeletedPayload.Create(guildChannel));
    }
}
