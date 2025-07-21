using GrillBot.App.Handlers.Logging;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Core.Infrastructure.Auth;
using GrillBot.Core.IO;
using GrillBot.Core.RabbitMQ.V2.Consumer;
using GrillBot.Core.RabbitMQ.V2.Publisher;
using GrillBot.Core.Services.GrillBot.Models.Events.Errors;
using GrillBot.Core.Services.GrillBot.Models.Events.Messages;
using GrillBot.Core.Services.GrillBot.Models.Events.Messages.Embeds;
using Microsoft.Extensions.Logging;

namespace GrillBot.App.Handlers.RabbitMQ;

public class ErrorNotificationEventHandler(
    ILoggerFactory loggerFactory,
    DataCacheManager _dataCache,
    IDiscordClient _discordClient,
    IRabbitPublisher _rabbitPublisher,
    WithoutAccidentRenderer _renderer,
    IConfiguration _configuration
) : RabbitMessageHandlerBase<ErrorNotificationPayload>(loggerFactory)
{
    protected override async Task<RabbitConsumptionResult> HandleInternalAsync(
        ErrorNotificationPayload message,
        ICurrentUserProvider currentUser,
        Dictionary<string, string> headers,
        CancellationToken cancellationToken = default
    )
    {
        await _discordClient.WaitOnConnectedState(cancellationToken);

        var withoutAccidentImage = await CreateWithoutAccidentImageAsync(message.UserId);

        try
        {
            await _dataCache.SetValueAsync("GrillBot_LastErrorDate", DateTime.Now, TimeSpan.FromDays(365));

            var msg = await CreateMessage(message, withoutAccidentImage);
            if (msg is null)
                return RabbitConsumptionResult.Success;

            await _rabbitPublisher.PublishAsync(msg);
        }
        finally
        {
            withoutAccidentImage?.Dispose();
        }

        return RabbitConsumptionResult.Success;
    }

    private async Task<TemporaryFile?> CreateWithoutAccidentImageAsync(ulong? userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = userId is null ? _discordClient.CurrentUser : await _discordClient.FindUserAsync(userId.Value, cancellationToken);
            return await _renderer.RenderAsync(user!);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private async Task<DiscordSendMessagePayload?> CreateMessage(ErrorNotificationPayload payload, TemporaryFile? withoutAccidentImage)
    {
        var loggingConfiguration = _configuration.GetSection("Discord:Logging");
        if (!loggingConfiguration.GetValue<bool>("Enabled"))
            return null;

        var guildId = loggingConfiguration.GetValue<ulong>("GuildId");
        var channelId = loggingConfiguration.GetValue<ulong>("ChannelId");
        var attachment = await CreateWithoutAccidentAttachmentAsync(withoutAccidentImage);
        var embed = CreateMessageEmbed(payload, attachment?.Filename);

        return new DiscordSendMessagePayload(
            guildId,
            channelId,
            null,
            attachment is null ? [] : [attachment],
            "GrillBot",
            null,
            null,
            embed
        );
    }

    private static async Task<DiscordMessageFile?> CreateWithoutAccidentAttachmentAsync(TemporaryFile? image)
    {
        if (image is null)
            return null;

        var content = await image.ReadAllBytesAsync();
        return new DiscordMessageFile(image.Filename, false, content);
    }

    private DiscordMessageEmbed CreateMessageEmbed(ErrorNotificationPayload payload, string? withoutAccidentName)
    {
        return new DiscordMessageEmbed(
            null,
            new(payload.Title ?? "", []),
            null,
            null,
            Color.Red.RawValue,
            DiscordMessageEmbedFooter.FromEmbed(new EmbedFooterBuilder().WithUser(_discordClient.CurrentUser)),
            withoutAccidentName is null ? null : new($"attachment://{withoutAccidentName}", []),
            null,
            payload.Fields.Select(o => new DiscordMessageEmbedField(o.Key, o.Value, o.IsInline)),
            null,
            true
        );
    }
}
