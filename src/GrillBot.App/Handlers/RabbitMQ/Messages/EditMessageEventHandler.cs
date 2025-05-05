using Discord.Net;
using GrillBot.App.Managers;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Common.Helpers;
using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Core.RabbitMQ.V2.Consumer;
using GrillBot.Core.RabbitMQ.V2.Publisher;
using GrillBot.Core.Services.GrillBot.Models.Events.Messages;
using Microsoft.Extensions.Logging;

namespace GrillBot.App.Handlers.RabbitMQ.Messages;

public class EditMessageEventHandler(
    ILoggerFactory loggerFactory,
    LocalizationManager localizationManager,
    IDiscordClient discordClient,
    IRabbitPublisher rabbitPublisher
) : MessageEventHandlerBase<DiscordEditMessagePayload>(loggerFactory, discordClient, localizationManager, rabbitPublisher)
{
    protected override async Task<RabbitConsumptionResult> HandleInternalAsync(DiscordEditMessagePayload message, ICurrentUserProvider currentUser, Dictionary<string, string> headers)
    {
        await DiscordClient.WaitOnConnectedState();

        var channel = await GetChannelAsync(message.GuildId, message.ChannelId);
        var embed = await CreateEmbedAsync(message);
        var allowedMentions = message.AllowedMentions?.ToAllowedMentions();
        var flags = message.Flags ?? MessageFlags.None;
        var components = message.Components?.BuildComponents();
        var wrappedComponents = components is null ? null : ComponentsHelper.CreateWrappedComponents(components.ToList().AsReadOnly());
        var content = await CreateContentAsync(message);
        var fileAttachments = message.Attachments.ConvertAll(o => o.ToFileAttachment());

        if (channel is null)
        {
            await LogWarningAsync(message.GuildId, message.ChannelId, $"Unable to find channel with ID {message.ChannelId}", message);
            return RabbitConsumptionResult.Success;
        }

        if (embed is null && string.IsNullOrEmpty(content) && message.Attachments.Count == 0)
        {
            await LogWarningAsync(message.GuildId, message.ChannelId, "Unable to edit discord message without content.", message);
            return RabbitConsumptionResult.Success;
        }

        try
        {
            await channel.ModifyMessageAsync(message.MessageId, props =>
            {
                props.AllowedMentions = allowedMentions;
                props.Flags = flags;
                props.Embed = embed;
                props.Content = content;
                props.Components = wrappedComponents;
                props.Attachments = fileAttachments;
            });
        }
        catch (HttpException ex) when (
            ex.DiscordCode is DiscordErrorCode.MaximumNumberOfEditsReached or DiscordErrorCode.CannotEditOtherUsersMessage or DiscordErrorCode.UnknownMessage
        )
        {
            return RabbitConsumptionResult.Success;
        }
        finally
        {
            foreach (var attachment in fileAttachments)
                attachment.Dispose();
        }

        return RabbitConsumptionResult.Success;
    }
}
