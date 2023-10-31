using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Models;
using GrillBot.Core.Infrastructure.Actions;
using GrillBot.Core.Services.AuditLog;
using GrillBot.Core.Services.AuditLog.Enums;
using GrillBot.Core.Services.AuditLog.Models.Request.CreateItems;

namespace GrillBot.App.Actions.Api.V1.Channel;

public class ClearMessageCache : ApiAction
{
    private IDiscordClient DiscordClient { get; }
    private IMessageCacheManager MessageCache { get; }
    private IAuditLogServiceClient AuditLogServiceClient { get; }

    public ClearMessageCache(ApiRequestContext apiContext, IDiscordClient discordClient, IMessageCacheManager messageCache, IAuditLogServiceClient auditLogServiceClient) : base(apiContext)
    {
        DiscordClient = discordClient;
        MessageCache = messageCache;
        AuditLogServiceClient = auditLogServiceClient;
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
        var logRequest = new LogRequest
        {
            Type = LogType.Info,
            ChannelId = channel.Id.ToString(),
            CreatedAt = DateTime.UtcNow,
            GuildId = guild.Id.ToString(),
            LogMessage = new LogMessageRequest
            {
                Message = $"Byla ručně smazána cache zpráv kanálu. Smazaných zpráv: {count}",
                Severity = LogSeverity.Info,
                SourceAppName = "GrillBot",
                Source = nameof(ClearMessageCache)
            },
            UserId = ApiContext.GetUserId().ToString()
        };

        await AuditLogServiceClient.CreateItemsAsync(new List<LogRequest> { logRequest });
        return ApiResult.Ok();
    }
}
