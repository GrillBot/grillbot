using GrillBot.App.Services.AuditLog;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Models;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Actions.Api.V1.Channel;

public class ClearMessageCache : ApiAction
{
    private IDiscordClient DiscordClient { get; }
    private IMessageCacheManager MessageCache { get; }
    private AuditLogWriter AuditLogWriter { get; }

    public ClearMessageCache(ApiRequestContext apiContext, IDiscordClient discordClient, IMessageCacheManager messageCache, AuditLogWriter auditLogWriter) : base(apiContext)
    {
        DiscordClient = discordClient;
        MessageCache = messageCache;
        AuditLogWriter = auditLogWriter;
    }

    public async Task ProcessAsync(ulong guildId, ulong channelId)
    {
        var guild = await DiscordClient.GetGuildAsync(guildId, CacheMode.CacheOnly);
        if (guild == null) return;

        var channel = await guild.GetChannelAsync(channelId);
        if (channel == null) return;

        var count = await MessageCache.ClearAllMessagesFromChannelAsync(channel);
        var logItem = new AuditLogDataWrapper(AuditLogItemType.Info, $"Byla ručně smazána cache zpráv kanálu. Smazaných zpráv: {count}", guild, channel, ApiContext.LoggedUser);
        await AuditLogWriter.StoreAsync(logItem);
    }
}
