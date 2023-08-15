using Discord;

namespace GrillBot.Cache.Services.Managers.MessageCache;

public interface IMessageCacheManager
{
    Task<int> GetCachedMessagesCount(IChannel channel);
    Task<IMessage?> GetAsync(ulong messageId, IMessageChannel? channel, bool includeRemoved = false, bool forceReload = false);
    Task<int> ClearAllMessagesFromChannelAsync(IChannel channel);
    Task<string> ProcessScheduledTaskAsync();
    Task DeleteAsync(ulong messageId);
}
