using Discord;

namespace GrillBot.Cache.Services.Managers.MessageCache;

public interface IMessageCacheManager
{
    Task DownloadMessagesAsync(IMessageChannel channel, int limit = DiscordConfig.MaxMessagesPerBatch);
    Task<int> GetCachedMessagesCount(IChannel channel);
    Task<IMessage?> GetAsync(ulong messageId, IMessageChannel? channel, bool includeRemoved = false, bool forceReload = false);
    Task<IMessage?> GetLastMessageAsync(IChannel? channel = null, IUser? author = null, IGuild? guild = null);
    Task<int> ClearAllMessagesFromChannelAsync(IChannel channel);
    Task<string> ProcessScheduledTaskAsync();
}
