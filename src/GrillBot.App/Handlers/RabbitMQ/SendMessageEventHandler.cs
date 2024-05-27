using GrillBot.Common.Extensions.Discord;
using GrillBot.Core.Extensions;
using GrillBot.Core.RabbitMQ.Consumer;
using GrillBot.Core.RabbitMQ.Publisher;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Events.Create;
using GrillBot.Core.Services.GrillBot.Models.Events;
using GrillBot.Core.Services.GrillBot.Models.Events.Messages;
using Microsoft.Extensions.Logging;

namespace GrillBot.App.Handlers.RabbitMQ;

public class SendMessageEventHandler : BaseRabbitMQHandler<DiscordMessagePayload>
{
    public override string QueueName => new DiscordMessagePayload().QueueName;

    private readonly IDiscordClient _discordClient;
    private readonly ILogger<SendMessageEventHandler> _logger;
    private readonly IRabbitMQPublisher _rabbitPublisher;

    public SendMessageEventHandler(ILoggerFactory loggerFactory, IDiscordClient discordClient, IRabbitMQPublisher rabbitPublisher) : base(loggerFactory)
    {
        _discordClient = discordClient;
        _logger = loggerFactory.CreateLogger<SendMessageEventHandler>();
        _rabbitPublisher = rabbitPublisher;
    }

    protected override async Task HandleInternalAsync(DiscordMessagePayload payload, Dictionary<string, string> headers)
    {
        await _discordClient.WaitOnConnectedState();

        var channel = await GetChannelAsync(payload);
        var embed = payload.Embed?.IsValidEmbed() == true ? payload.Embed.ToBuilder().Build() : null;
        var allowedMentions = payload.AllowedMentions?.ToAllowedMentions();
        var flags = payload.Flags ?? MessageFlags.None;

        if (channel is null)
        {
            await LogWarningAsync(payload, $"Unable to find channel with ID {payload.ChannelId}");
            return;
        }

        if (embed is null && string.IsNullOrEmpty(payload.Content) && payload.Attachments.Count == 0)
        {
            await LogWarningAsync(payload, "Unable to send discord message without content.");
            return;
        }

        IUserMessage? message = null;
        try
        {
            if (payload.Attachments.Count > 0)
            {
                message = await SendMessageWithAttachmentsAsync(channel, payload.Content, allowedMentions, payload.Attachments, embed, flags);
                return;
            }

            message = await channel.SendMessageAsync(
                text: payload.Content,
                isTTS: false,
                embed: embed,
                options: null,
                allowedMentions: allowedMentions,
                messageReference: null,
                components: null,
                stickers: null,
                embeds: null,
                flags: flags
            );
        }
        finally
        {
            if (message is not null)
                await PublishCreatedMessageAsync(payload, message);
        }
    }

    private static async Task<IUserMessage> SendMessageWithAttachmentsAsync(IMessageChannel channel, string? content, AllowedMentions? allowedMentions, List<DiscordMessageFile> attachments,
        Embed? embed, MessageFlags flags)
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
                components: null,
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
        await _rabbitPublisher.PublishAsync(new CreateItemsPayload(logRequest), new());
    }

    private async Task PublishCreatedMessageAsync(DiscordMessagePayload payload, IUserMessage message)
    {
        var createdMessagePayload = new CreatedDiscordMessagePayload(payload.GuildId, payload.ChannelId, message.Id.ToString(), payload.ServiceId, payload.ServiceData);
        await _rabbitPublisher.PublishAsync(createdMessagePayload);
    }
}
