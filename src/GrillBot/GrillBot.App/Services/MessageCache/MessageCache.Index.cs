using GrillBot.Cache.Entity;
using GrillBot.Common.Extensions;

namespace GrillBot.App.Services.MessageCache;

public partial class MessageCache
{
    private Task<List<ulong>> GetMessageIdsAsync(IUser author = null, IChannel channel = null, IGuild guild = null)
        => GetMessageIdsAsync(author?.Id ?? 0, channel?.Id ?? 0, guild?.Id ?? 0);

    private async Task<List<ulong>> GetMessageIdsAsync(ulong authorId = 0, ulong channelId = 0, ulong guildId = 0)
    {
        await IndexLock.WaitAsync();

        try
        {
            using var cache = CacheBuilder.CreateRepository();

            var indexes = await cache.MessageIndexRepository.GetMessagesAsync(authorId, channelId, guildId);
            return indexes.ConvertAll(o => o.MessageId.ToUlong());
        }
        finally
        {
            IndexLock.Release();
        }
    }

    public Task<int> GetMessagesCountAsync(IUser author = null, IChannel channel = null, IGuild guild = null)
        => GetMessagesCountAsync(author?.Id ?? 0, channel?.Id ?? 0, guild?.Id ?? 0);

    public async Task<int> GetMessagesCountAsync(ulong authorId = 0, ulong channelId = 0, ulong guildId = 0)
    {
        await IndexLock.WaitAsync();

        try
        {
            using var cache = CacheBuilder.CreateRepository();
            return await cache.MessageIndexRepository.GetMessagesCountAsync(authorId, channelId, guildId);
        }
        finally
        {
            IndexLock.Release();
        }
    }

    private Task RemoveIndexAsync(IMessage message)
        => RemoveIndexAsync(message.Id);

    private async Task RemoveIndexAsync(ulong messageId)
    {
        await IndexLock.WaitAsync();

        try
        {
            var cache = CacheBuilder.CreateRepository();
            var index = await cache.MessageIndexRepository.FindMessageByIdAsync(messageId);

            if (index != null)
            {
                cache.Remove(index);
                await cache.CommitAsync();
            }
        }
        finally
        {
            IndexLock.Release();
        }
    }

    private Task CreateIndexAsync(IMessage message)
        => CreateIndexAsync(message.Id, message.Channel.Id, message.Author.Id, (message.Channel as IGuildChannel)?.GuildId ?? 0);

    private async Task CreateIndexAsync(ulong messageId, ulong channelId, ulong authorId, ulong guildId)
    {
        var entity = new MessageIndex()
        {
            AuthorId = authorId.ToString(),
            ChannelId = channelId.ToString(),
            GuildId = guildId.ToString(),
            MessageId = messageId.ToString()
        };

        await IndexLock.WaitAsync();

        try
        {
            using var cache = CacheBuilder.CreateRepository();

            await cache.AddAsync(entity);
            await cache.CommitAsync();
        }
        finally
        {
            IndexLock.Release();
        }
    }

    private async Task CreateIndexesAsync(IEnumerable<IMessage> messages)
    {
        var entities = messages.Select(o => new MessageIndex()
        {
            AuthorId = o.Author.Id.ToString(),
            ChannelId = o.Channel.Id.ToString(),
            MessageId = o.Id.ToString(),
            GuildId = ((o.Channel as IGuildChannel)?.GuildId ?? 0).ToString()
        }).ToList();

        await IndexLock.WaitAsync();

        try
        {
            using var cache = CacheBuilder.CreateRepository();

            await cache.AddRangeAsync(entities);
            await cache.CommitAsync();
        }
        finally
        {
            IndexLock.Release();
        }
    }
}
