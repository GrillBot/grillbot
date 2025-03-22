using Discord.Net;
using GrillBot.App.Managers;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Common.Managers.Localization;
using GrillBot.Core.Extensions;
using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Core.RabbitMQ.V2.Consumer;
using GrillBot.Core.RabbitMQ.V2.Publisher;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Events.Create;
using GrillBot.Core.Services.GrillBot.Models.Events.Messages;
using GrillBot.Data.Models;
using Microsoft.Extensions.Logging;

namespace GrillBot.App.Handlers.RabbitMQ;

public class SendMessageEventHandler : RabbitMessageHandlerBase<DiscordMessagePayload>
{
    private readonly IDiscordClient _discordClient;
    private readonly ILogger<SendMessageEventHandler> _logger;
    private readonly IRabbitPublisher _rabbitPublisher;
    private readonly LocalizedEmbedManager _localizedEmbedManager;

    public SendMessageEventHandler(ILoggerFactory loggerFactory, IDiscordClient discordClient, IRabbitPublisher rabbitPublisher,
        LocalizedEmbedManager localizedEmbedManager) : base(loggerFactory)
    {
        _discordClient = discordClient;
        _logger = loggerFactory.CreateLogger<SendMessageEventHandler>();
        _rabbitPublisher = rabbitPublisher;
        _localizedEmbedManager = localizedEmbedManager;
    }

    protected override async Task<RabbitConsumptionResult> HandleInternalAsync(DiscordMessagePayload message, ICurrentUserProvider currentUser, Dictionary<string, string> headers)
    {
        await _discordClient.WaitOnConnectedState();

        var channel = await GetChannelAsync(message);
        var embed = await CreateEmbedAsync(message);
        var allowedMentions = message.AllowedMentions?.ToAllowedMentions();
        var flags = message.Flags ?? MessageFlags.None;
        var components = message.Components?.BuildComponents();
        var wrappedComponents = components is null ? null : ComponentsHelper.CreateWrappedComponents(components.ToList().AsReadOnly());

        if (channel is null)
        {
            await LogWarningAsync(message, $"Unable to find channel with ID {message.ChannelId}");
            return RabbitConsumptionResult.Success;
        }

        if (embed is null && string.IsNullOrEmpty(message.Content) && message.Attachments.Count == 0)
        {
            await LogWarningAsync(message, "Unable to send discord message without content.");
            return RabbitConsumptionResult.Success;
        }

        IUserMessage? msg = null;
        try
        {
            if (message.Attachments.Count > 0)
            {
                msg = await SendMessageWithAttachmentsAsync(channel, message.Content, allowedMentions, message.Attachments, embed, flags, wrappedComponents);
                return RabbitConsumptionResult.Success;
            }

            msg = await channel.SendMessageAsync(
                text: message.Content,
                isTTS: false,
                embed: embed,
                options: null,
                allowedMentions: allowedMentions,
                messageReference: null,
                components: wrappedComponents,
                stickers: null,
                embeds: null,
                flags: flags
            );
        }
        catch (HttpException ex) when (ex.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
        {
            msg = new EmptyUserMessage(0);
        }
        finally
        {
            if (message is not null)
                await PublishCreatedMessageAsync(message, msg!);
        }

        return RabbitConsumptionResult.Success;
    }

    private static async Task<IUserMessage> SendMessageWithAttachmentsAsync(IMessageChannel channel, string? content, AllowedMentions? allowedMentions, List<DiscordMessageFile> attachments,
        Embed? embed, MessageFlags flags, MessageComponent? components)
    {
        var fileAttachments = attachments.ConvertAll(o => o.ToFileAttachment());

        try
        {
            return await channel.SendFilesAsync(
                attachments: fileAttachments,
                text: content,
                isTTS: false,
                embed: embed,
                options: null,
                allowedMentions: allowedMentions,
                messageReference: null,
                components: components,
                stickers: null,
                embeds: null,
                flags: flags
            );
        }
        finally
        {
            foreach (var attachment in fileAttachments)
                attachment.Dispose();
        }
    }

    private async Task<IMessageChannel?> GetChannelAsync(DiscordMessagePayload payload)
    {
        if (!string.IsNullOrEmpty(payload.GuildId))
        {
            var guild = await _discordClient.GetGuildAsync(payload.GuildId.ToUlong());
            return guild is null ? null : await guild.GetTextChannelAsync(payload.ChannelId.ToUlong());
        }

        var user = await _discordClient.GetUserAsync(payload.ChannelId.ToUlong());
        return user is null ? null : (IMessageChannel)await user.CreateDMChannelAsync();
    }

    private async Task LogWarningAsync(DiscordMessagePayload payload, string message)
    {
        var logRequest = new LogRequest(LogType.Warning, DateTime.UtcNow, payload.GuildId, _discordClient.CurrentUser.Id.ToString(), payload.ChannelId)
        {
            LogMessage = new LogMessageRequest($"{message}\n{System.Text.Json.JsonSerializer.Serialize(payload)}", LogSeverity.Warning, "GrillBot", $"RabbitMQ/{QueueName}/{GetType().Name}")
        };

        _logger.LogWarning("{message}", message);
        await _rabbitPublisher.PublishAsync(new CreateItemsMessage(logRequest));
    }

    private async Task PublishCreatedMessageAsync(DiscordMessagePayload payload, IUserMessage message)
    {
        var createdMessagePayload = new CreatedDiscordMessagePayload(payload.GuildId, payload.ChannelId, message.Id.ToString(), payload.ServiceId, payload.ServiceData);
        await _rabbitPublisher.PublishAsync(createdMessagePayload);
    }

    private async Task<Embed?> CreateEmbedAsync(DiscordMessagePayload payload)
    {
        if (payload.Embed is null)
            return null;

        var embedBuilder = payload.Embed.ToBuilder();
        var isOriginalEmbedValid = payload.Embed.IsValidEmbed();
        var canLocalizeEmbed = payload.ServiceData.TryGetValue("UseLocalizedEmbeds", out var useLocalizedEmbeds) || useLocalizedEmbeds != "true";

        if (!canLocalizeEmbed)
            return !isOriginalEmbedValid ? null : embedBuilder.Build();

        if (!payload.ServiceData.TryGetValue("Language", out var language))
            language = TextsManager.DefaultLocale;

        var localizedEmbed = await _localizedEmbedManager.CreateLocalizedEmbedAsync(embedBuilder, language, payload.ServiceData);
        return localizedEmbed.Build();
    }
}
