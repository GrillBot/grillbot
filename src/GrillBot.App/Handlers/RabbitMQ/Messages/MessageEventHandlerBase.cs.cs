using GrillBot.App.Managers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Core.RabbitMQ.V2.Consumer;
using GrillBot.Core.RabbitMQ.V2.Messages;
using GrillBot.Core.RabbitMQ.V2.Publisher;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Events.Create;
using GrillBot.Core.Services.GrillBot.Models.Events.Messages;
using Microsoft.Extensions.Logging;

namespace GrillBot.App.Handlers.RabbitMQ.Messages;

public abstract class MessageEventHandlerBase<TPayload>(
    ILoggerFactory loggerFactory,
    IDiscordClient _discordClient,
    LocalizationManager _localizationManager,
    IRabbitPublisher _rabbitPublisher
) : RabbitMessageHandlerBase<TPayload>(loggerFactory)
    where TPayload : DiscordMessagePayloadData, IRabbitMessage, new()
{
    protected IDiscordClient DiscordClient => _discordClient;
    protected IRabbitPublisher RabbitPublisher => _rabbitPublisher;

    protected async Task<IMessageChannel?> GetChannelAsync(ulong? guildId, ulong channelId)
    {
        if (guildId is not null)
        {
            var guild = await _discordClient.GetGuildAsync(guildId.Value);
            return guild is null ? null : await guild.GetTextChannelAsync(channelId);
        }

        var user = await _discordClient.GetUserAsync(channelId);
        return user is null ? null : (IMessageChannel)await user.CreateDMChannelAsync();
    }

    protected async Task LogWarningAsync(ulong? guildId, ulong channelId, string message, TPayload payload)
    {
        var logRequest = new LogRequest(LogType.Warning, DateTime.UtcNow, guildId?.ToString(), _discordClient.CurrentUser.Id.ToString(), channelId.ToString())
        {
            LogMessage = new LogMessageRequest
            {
                Message = $"{message}\n{System.Text.Json.JsonSerializer.Serialize(payload)}",
                SourceAppName = "GrillBot",
                Source = $"RabbitMQ/{QueueName}/{GetType().Name}"
            }
        };

        Logger.LogWarning("{Message}", message);
        await RabbitPublisher.PublishAsync(new CreateItemsMessage(logRequest));
    }

    protected async Task<Embed?> CreateEmbedAsync(TPayload payload)
    {
        if (payload.Embed is null)
            return null;

        var embedBuilder = payload.Embed.ToBuilder();
        var isOriginalEmbedValid = payload.Embed.IsValidEmbed();

        if (!payload.CanUseLocalization)
            return !isOriginalEmbedValid ? null : embedBuilder.Build();

        if (!payload.ServiceData.TryGetValue("Language", out var language))
            language = TextsManager.DefaultLocale;

        var localizedEmbed = await _localizationManager.CreateLocalizedEmbedAsync(embedBuilder, language, payload.ServiceData);
        return localizedEmbed.Build();
    }

    protected async Task<string?> CreateContentAsync(TPayload payload)
    {
        if (string.IsNullOrEmpty(payload.Content))
            return null;
        if (!payload.CanUseLocalization)
            return payload.Content;

        var locale = payload.Locale ?? TextsManager.DefaultLocale;
        return await _localizationManager.TransformValueAsync(payload.Content!, locale, payload.ServiceData);
    }
}
