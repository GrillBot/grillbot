using GrillBot.App.Handlers.Logging;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Extensions.Discord;
using GrillBot.Core.IO;
using GrillBot.Core.RabbitMQ.Consumer;
using GrillBot.Core.RabbitMQ.Publisher;
using GrillBot.Core.Services.GrillBot.Models.Events.Errors;
using GrillBot.Core.Services.GrillBot.Models.Events.Messages;
using Microsoft.Extensions.Logging;

namespace GrillBot.App.Handlers.RabbitMQ;

public class ErrorNotificationEventHandler : BaseRabbitMQHandler<ErrorNotificationPayload>
{
    public override string QueueName => new ErrorNotificationPayload().QueueName;

    private readonly DataCacheManager _dataCache;
    private readonly IDiscordClient _discordClient;
    private readonly WithoutAccidentRenderer _renderer;
    private readonly IRabbitMQPublisher _rabbitPublisher;
    private readonly IConfiguration _configuration;

    public ErrorNotificationEventHandler(ILoggerFactory loggerFactory, DataCacheManager dataCache, IDiscordClient discordClient, IRabbitMQPublisher rabbitPublisher,
        WithoutAccidentRenderer renderer, IConfiguration configuration) : base(loggerFactory)
    {
        _dataCache = dataCache;
        _discordClient = discordClient;
        _rabbitPublisher = rabbitPublisher;
        _renderer = renderer;
        _configuration = configuration;
    }

    protected override async Task HandleInternalAsync(ErrorNotificationPayload payload, Dictionary<string, string> headers)
    {
        await _discordClient.WaitOnConnectedState();

        var withoutAccidentImage = await CreateWithoutAccidentImageAsync(payload.UserId);

        try
        {
            await _dataCache.SetValueAsync("LastErrorDate", DateTime.Now.ToString("o"), DateTime.MaxValue);

            var message = await CreateMessage(payload, withoutAccidentImage);
            if (message is null)
                return;

            await _rabbitPublisher.PublishAsync(message, new());
        }
        finally
        {
            withoutAccidentImage?.Dispose();
        }
    }

    private async Task<TemporaryFile?> CreateWithoutAccidentImageAsync(ulong? userId)
    {
        try
        {
            var user = userId is null ? _discordClient.CurrentUser : await _discordClient.FindUserAsync(userId.Value);
            return await _renderer.RenderAsync(user!);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private async Task<DiscordMessagePayload?> CreateMessage(ErrorNotificationPayload payload, TemporaryFile? withoutAccidentImage)
    {
        var loggingConfiguration = _configuration.GetSection("Discord:Logging");
        if (!loggingConfiguration.GetValue<bool>("Enabled"))
            return null;

        var guildId = loggingConfiguration.GetValue<ulong>("GuildId").ToString();
        var channelId = loggingConfiguration.GetValue<ulong>("ChannelId").ToString();
        var attachment = await CreateWithoutAccidentAttachmentAsync(withoutAccidentImage);
        var embed = CreateMessageEmbed(payload, attachment?.Filename);

        return new DiscordMessagePayload(
            guildId,
            channelId,
            null,
            attachment is null ? Enumerable.Empty<DiscordMessageFile>() : new[] { attachment },
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

        var content = await File.ReadAllBytesAsync(image.Path);
        var filename = Path.GetFileName(image.Path);
        return new DiscordMessageFile(filename, false, content);
    }

    private DiscordMessageEmbed CreateMessageEmbed(ErrorNotificationPayload payload, string? withoutAccidentName)
    {
        return new DiscordMessageEmbed(
            null,
            payload.Title,
            null,
            null,
            Color.Red.RawValue,
            DiscordMessageEmbedFooter.FromEmbed(new EmbedFooterBuilder().WithUser(_discordClient.CurrentUser)),
            withoutAccidentName is null ? null : $"attachment://{withoutAccidentName}",
            null,
            payload.Fields.Select(o => new DiscordMessageEmbedField(o.Key, o.Value, o.IsInline)),
            null,
            true
        );
    }
}
