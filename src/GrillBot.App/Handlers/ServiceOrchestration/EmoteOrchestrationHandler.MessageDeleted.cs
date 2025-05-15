using GrillBot.Common.Extensions.Discord;
using GrillBot.Core.Services.Emote.Models.Events;
using GrillBot.Core.Services.Emote.Models.Events.Suggestions;

namespace GrillBot.App.Handlers.ServiceOrchestration;

public partial class EmoteOrchestrationHandler
{
    // MessageDeleted
    public async Task ProcessAsync(Cacheable<IMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> cachedChannel)
    {
        if (!cachedChannel.HasValue || cachedChannel.Value is not ITextChannel textChannel)
            return;

        var message = cachedMessage.HasValue ? cachedMessage.Value : null;
        message ??= await _messageCache.GetAsync(cachedMessage.Id, null, true) as IUserMessage;
        if (message is null || message.Author is not IGuildUser guildUser)
            return;

        await ProcessEmotesAsync(message, guildUser);
        await ProcessEmoteSuggestionsAsync(message, textChannel.Guild);
    }

    private async Task ProcessEmotesAsync(IMessage message, IGuildUser guildUser)
    {
        if (message.IsCommand(_discordClient.CurrentUser))
            return;

        var emotes = message.GetEmotesFromMessage().ToList();
        if (emotes.Count == 0)
            return;

        var guildId = guildUser.GuildId.ToString();
        var userId = guildUser.Id.ToString();
        var createdAt = DateTime.UtcNow;
        var payloads = emotes.ConvertAll(e => new EmoteEventPayload(guildId, userId, e.ToString(), createdAt, false));

        await _rabbitPublisher.PublishAsync(payloads);
    }

    private Task ProcessEmoteSuggestionsAsync(IMessage message, IGuild guild)
    {
        return message.Embeds.Count == 0 ?
            Task.CompletedTask :
            _rabbitPublisher.PublishAsync(new EmoteSuggestionMessageDeletedPayload(guild.Id, message.Id));
    }
}
