using Discord.Net;
using GrillBot.App.Managers;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Core.RabbitMQ.V2.Consumer;
using GrillBot.Core.RabbitMQ.V2.Publisher;
using GrillBot.Core.Services.GrillBot.Models.Events.Messages;
using GrillBot.Data.Models;
using Microsoft.Extensions.Logging;

namespace GrillBot.App.Handlers.RabbitMQ.Messages;

public class SendMessageEventHandler(
    ILoggerFactory loggerFactory,
    IDiscordClient discordClient,
    IRabbitPublisher rabbitPublisher,
    LocalizationManager localizationManager
) : MessageEventHandlerBase<DiscordSendMessagePayload>(loggerFactory, discordClient, localizationManager, rabbitPublisher)
{
    protected override async Task<RabbitConsumptionResult> HandleInternalAsync(
        DiscordSendMessagePayload message,
        ICurrentUserProvider currentUser,
        Dictionary<string, string> headers,
        CancellationToken cancellationToken = default
    )
    {
        await DiscordClient.WaitOnConnectedState(cancellationToken);

        var channel = await GetChannelAsync(message.GuildId, message.ChannelId);
        var embed = await CreateEmbedAsync(message);
        var allowedMentions = message.AllowedMentions?.ToAllowedMentions();
        var flags = message.Flags ?? MessageFlags.None;
        var components = (message.Components?.BuildComponents() ?? []).Select(o => o.ToBuilder());
        var wrappedComponents = components is null ? null : ComponentsHelper.CreateWrappedComponents(components.ToList().AsReadOnly());
        var content = await CreateContentAsync(message);
        var reference = message.Reference?.ToDiscordReference();

        if (channel is null)
        {
            await LogWarningAsync(message.GuildId, message.ChannelId, $"Unable to find channel with ID {message.ChannelId}", message);
            return RabbitConsumptionResult.Success;
        }

        if (embed is null && string.IsNullOrEmpty(content) && message.Attachments.Count == 0)
        {
            await LogWarningAsync(message.GuildId, message.ChannelId, "Unable to send discord message without content.", message);
            return RabbitConsumptionResult.Success;
        }

        IUserMessage? msg = null;
        try
        {
            if (message.Attachments.Count > 0)
            {
                msg = await SendMessageWithAttachmentsAsync(channel, content, allowedMentions, message.Attachments, embed, flags, wrappedComponents, reference);
                return RabbitConsumptionResult.Success;
            }

            msg = await channel.SendMessageAsync(
                text: content,
                isTTS: false,
                embed: embed,
                options: null,
                allowedMentions: allowedMentions,
                messageReference: reference,
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
        Embed? embed, MessageFlags flags, MessageComponent? components, MessageReference? reference)
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
                messageReference: reference,
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

    private Task PublishCreatedMessageAsync(DiscordSendMessagePayload payload, IUserMessage message)
    {
        return RabbitPublisher.PublishAsync(new CreatedDiscordMessagePayload(
            payload.GuildId?.ToString(),
            payload.ChannelId.ToString(),
            message.Id.ToString(),
            payload.ServiceId,
            payload.ServiceData
        ));
    }
}
