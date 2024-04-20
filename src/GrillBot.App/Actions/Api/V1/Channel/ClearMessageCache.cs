using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.RabbitMQ.Publisher;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Events.Create;

namespace GrillBot.App.Actions.Api.V1.Channel;

public class ClearMessageCache : ApiAction
{
    private IDiscordClient DiscordClient { get; }
    private IMessageCacheManager MessageCache { get; }
    private IRabbitMQPublisher RabbitPublisher { get; }

    public ClearMessageCache(ApiRequestContext apiContext, IDiscordClient discordClient, IMessageCacheManager messageCache, IRabbitMQPublisher rabbitPublisher) : base(apiContext)
    {
        DiscordClient = discordClient;
        MessageCache = messageCache;
        RabbitPublisher = rabbitPublisher;
    }

    public override async Task<ApiResult> ProcessAsync()
    {
        var guildId = (ulong)Parameters[0]!;
        var channelId = (ulong)Parameters[1]!;

        var guild = await DiscordClient.GetGuildAsync(guildId, CacheMode.CacheOnly);
        if (guild == null)
            return ApiResult.Ok();

        var channel = await guild.GetChannelAsync(channelId);
        if (channel == null)
            return ApiResult.Ok();

        var count = await MessageCache.ClearAllMessagesFromChannelAsync(channel);
        var userId = ApiContext.GetUserId().ToString();
        var logRequest = new LogRequest(LogType.Info, DateTime.UtcNow, guildId.ToString(), userId, channelId.ToString())
        {
            LogMessage = new LogMessageRequest
            {
                Message = $"Byla ručně smazána cache zpráv kanálu. Smazaných zpráv: {count}",
                Severity = LogSeverity.Info,
                Source = nameof(ClearMessageCache),
                SourceAppName = "GrillBot"
            }
        };

        await RabbitPublisher.PublishAsync(new CreateItemsPayload(new() { logRequest }), new());
        return ApiResult.Ok();
    }
}
