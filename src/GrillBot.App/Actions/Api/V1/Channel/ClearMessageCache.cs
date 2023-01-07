using GrillBot.App.Managers;
using GrillBot.Cache.Services.Managers.MessageCache;
using GrillBot.Common.Models;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Database.Enums;

namespace GrillBot.App.Actions.Api.V1.Channel;

public class ClearMessageCache : ApiAction
{
    private IDiscordClient DiscordClient { get; }
    private IMessageCacheManager MessageCache { get; }
    private AuditLogWriteManager AuditLogWriteManager { get; }

    public ClearMessageCache(ApiRequestContext apiContext, IDiscordClient discordClient, IMessageCacheManager messageCache, AuditLogWriteManager auditLogWriteManager) : base(apiContext)
    {
        DiscordClient = discordClient;
        MessageCache = messageCache;
        AuditLogWriteManager = auditLogWriteManager;
    }

    public async Task ProcessAsync(ulong guildId, ulong channelId)
    {
        var guild = await DiscordClient.GetGuildAsync(guildId, CacheMode.CacheOnly);
        if (guild == null) return;

        var channel = await guild.GetChannelAsync(channelId);
        if (channel == null) return;

        var count = await MessageCache.ClearAllMessagesFromChannelAsync(channel);
        var logItem = new AuditLogDataWrapper(AuditLogItemType.Info, $"Byla ručně smazána cache zpráv kanálu. Smazaných zpráv: {count}", guild, channel, ApiContext.LoggedUser);
        await AuditLogWriteManager.StoreAsync(logItem);
    }
}
