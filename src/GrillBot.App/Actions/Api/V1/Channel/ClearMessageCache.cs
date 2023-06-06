using AuditLogService.Models.Request;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Models;
using GrillBot.Common.Services.AuditLog;
using GrillBot.Common.Services.AuditLog.Enums;
using GrillBot.Common.Services.AuditLog.Models;

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

    public async Task ProcessAsync(ulong guildId, ulong channelId)
    {
        var guild = await DiscordClient.GetGuildAsync(guildId, CacheMode.CacheOnly);
        if (guild == null) return;

        var channel = await guild.GetChannelAsync(channelId);
        if (channel == null) return;

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
                Severity = LogSeverity.Info
            },
            UserId = ApiContext.GetUserId().ToString()
        };

        await AuditLogServiceClient.CreateItemsAsync(new List<LogRequest> { logRequest });
    }
}
