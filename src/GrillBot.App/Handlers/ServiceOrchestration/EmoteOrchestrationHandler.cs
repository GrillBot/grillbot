using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Managers.Events.Contracts;
using GrillBot.Core.Extensions;
using GrillBot.Core.RabbitMQ.Publisher;
using GrillBot.Core.Services.Emote;
using GrillBot.Core.Services.Emote.Models.Events;

namespace GrillBot.App.Handlers.ServiceOrchestration;

public class EmoteOrchestrationHandler : IMessageDeletedEvent, IMessageReceivedEvent, IReactionAddedEvent, IReactionRemovedEvent, IReadyEvent, IGuildAvailableEvent, IGuildUpdatedEvent
{
    private readonly IRabbitMQPublisher _rabbitPublisher;
    private readonly IMessageCacheManager _messageCache;
    private readonly IDiscordClient _discordClient;
    private readonly IEmoteServiceClient _emoteServiceClient;

    public EmoteOrchestrationHandler(IRabbitMQPublisher rabbitPublisher, IMessageCacheManager messageCache, IDiscordClient discordClient, IEmoteServiceClient emoteServiceClient)
    {
        _rabbitPublisher = rabbitPublisher;
        _messageCache = messageCache;
        _discordClient = discordClient;
        _emoteServiceClient = emoteServiceClient;
    }

    // MessageDeleted
    public async Task ProcessAsync(Cacheable<IMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel)
    {
        if (!cachedChannel.HasValue || cachedChannel.Value is not ITextChannel)
            return;

        var message = cachedMessage.HasValue ? cachedMessage.Value : null;
        message ??= await _messageCache.GetAsync(cachedMessage.Id, null, true) as IUserMessage;
        if (message?.IsCommand(_discordClient.CurrentUser) != false) return;
        if (message.Author is not IGuildUser guildUser) return;

        var emotes = message.GetEmotesFromMessage().ToList();
        if (emotes.Count == 0) return;

        var guildId = guildUser.GuildId.ToString();
        var userId = guildUser.Id.ToString();
        var createdAt = DateTime.UtcNow;
        var payloads = emotes.ConvertAll(e => new EmoteEventPayload(guildId, userId, e.ToString(), createdAt, false));

        await _rabbitPublisher.PublishBatchAsync(payloads);
    }

    // MessageReceived
    public async Task ProcessAsync(IMessage message)
    {
        if (!message.TryLoadMessage(out var userMessage)) return; // Ignore messages from bots.
        if (string.IsNullOrEmpty(userMessage?.Content)) return; // Ignore empty messages.
        if (message.IsCommand(_discordClient.CurrentUser)) return; // Ignore commands.
        if (message.Channel is not ITextChannel textChannel) return; // Ignore DMs
        if (message.Author is not IGuildUser guildUser) return; // Ignore non guild users.

        var payloads = new List<EmoteEventPayload>();
        var guildId = textChannel.GuildId.ToString();
        var userId = guildUser.Id.ToString();
        var createdAt = DateTime.UtcNow;

        foreach (var emote in message.GetEmotesFromMessage())
        {
            var emoteId = emote.ToString();
            if (!await _emoteServiceClient.GetIsEmoteSupportedAsync(guildId, emoteId))
                continue;

            payloads.Add(new EmoteEventPayload(guildId, userId, emoteId, createdAt, true));
        }

        if (payloads.Count > 0)
            await _rabbitPublisher.PublishBatchAsync(payloads);
    }

    // ReactionAdded
    public async Task ProcessAsync(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel, SocketReaction reaction)
    {
        if (!cachedChannel.HasValue || cachedChannel.Value is not ITextChannel textChannel) return;
        if (reaction.Emote is not Emote emote) return;

        var message = cachedMessage.HasValue ? cachedMessage.Value : null;
        message ??= await _messageCache.GetAsync(cachedMessage.Id, null, true) as IUserMessage;
        if (message is not { Author: IGuildUser author } || author.Id == reaction.UserId) return;

        var reactionUser = reaction.User.IsSpecified ? reaction.User.Value as IGuildUser : null;
        reactionUser ??= await textChannel.Guild.GetUserAsync(reaction.UserId);
        if (reactionUser is not IGuildUser reactionGuildUser) return;

        var guildId = reactionGuildUser.GuildId.ToString();
        var userId = reactionGuildUser.Id.ToString();
        var emoteId = emote.ToString();

        if (await _emoteServiceClient.GetIsEmoteSupportedAsync(guildId, emoteId))
            await _rabbitPublisher.PublishAsync(new EmoteEventPayload(guildId, userId, emoteId, DateTime.UtcNow, true));
    }

    // ReactionRemoved
    async Task IReactionRemovedEvent.ProcessAsync(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel, SocketReaction reaction)
    {
        if (!cachedChannel.HasValue || cachedChannel.Value is not ITextChannel textChannel) return;
        if (reaction.Emote is not Emote emote) return;

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

    // Ready
    async Task IReadyEvent.ProcessAsync()
    {
        var guilds = await _discordClient.GetGuildsAsync();
        var payloads = new List<SynchronizeEmotesPayload>();

        foreach (var guild in guilds)
        {
            var emotes = guild.Emotes.ToList();
            payloads.Add(new SynchronizeEmotesPayload(guild.Id.ToString(), emotes));
        }

        if (payloads.Count > 0)
            await _rabbitPublisher.PublishBatchAsync(payloads);
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
}
