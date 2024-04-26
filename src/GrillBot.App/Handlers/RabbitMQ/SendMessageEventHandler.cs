using GrillBot.Common.Extensions.Discord;
using GrillBot.Core.Extensions;
using GrillBot.Core.RabbitMQ.Consumer;
using GrillBot.Core.Services.GrillBot.Models.Events;
using Microsoft.Extensions.Logging;

namespace GrillBot.App.Handlers.RabbitMQ;

public class SendMessageEventHandler : BaseRabbitMQHandler<DiscordMessagePayload>
{
    public override string QueueName => new DiscordMessagePayload().QueueName;

    private readonly IDiscordClient _discordClient;
    private readonly ILogger<SendMessageEventHandler> _logger;

    public SendMessageEventHandler(ILoggerFactory loggerFactory, IDiscordClient discordClient) : base(loggerFactory)
    {
        _discordClient = discordClient;
        _logger = loggerFactory.CreateLogger<SendMessageEventHandler>();
    }

    protected override async Task HandleInternalAsync(DiscordMessagePayload payload, Dictionary<string, string> headers)
    {
        await _discordClient.WaitOnConnectedState();

        var channel = await GetChannelAsync(payload);
        var embed = payload.Embed?.IsValidEmbed() == true ? payload.Embed.ToBuilder().Build() : null;
        var allowedAttachments = payload.AllowedMentions?.ToAllowedMentions();
        var flags = payload.Flags ?? MessageFlags.None;

        if (channel is null)
        {
            _logger.LogWarning("Unable to find channel with ID {ChannelId}", payload.ChannelId);
            return; // TODO Send Error to AuditLog.
        }

        if (embed is null && string.IsNullOrEmpty(payload.Content) && payload.Attachments.Count == 0)
        {
            _logger.LogWarning("Unable to send discord message without content.");
            return; // TODO Send Error to AuditLog.
        }

        if (payload.Attachments.Count > 0)
        {
            await SendMessageWithAttachmentsAsync(channel, payload.Content, allowedAttachments, payload.Attachments, embed, flags);
            return;
        }

        await channel.SendMessageAsync(
            text: payload.Content,
            isTTS: false,
            embed: embed,
            options: null,
            allowedMentions: allowedAttachments,
            messageReference: null,
            components: null,
            stickers: null,
            embeds: null,
            flags: flags
        );
    }

    private static async Task SendMessageWithAttachmentsAsync(IMessageChannel channel, string? content, AllowedMentions? allowedMentions, List<DiscordMessageFile> attachments,
        Embed? embed, MessageFlags flags)
    {
        var fileAttachments = attachments.ConvertAll(o => o.ToFileAttachment());

        try
        {
            await channel.SendFilesAsync(
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
}
