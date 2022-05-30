using GrillBot.Cache.Entity;
using GrillBot.Common.Managers.Counters;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Cache.Services.Repository;

public class MessageIndexRepository : RepositoryBase
{
    public MessageIndexRepository(GrillBotCacheContext context, CounterManager counter) : base(context, counter)
    {
    }

    private IQueryable<MessageIndex> GetBaseQuery(ulong authorId = default, ulong channelId = default, ulong guildId = default)
    {
        var query = Context.MessageIndex.AsQueryable();

        if (authorId != default) query = query.Where(o => o.AuthorId == authorId.ToString());
        if (channelId != default) query = query.Where(o => o.ChannelId == channelId.ToString());
        if (guildId != default) query = query.Where(o => o.GuildId == guildId.ToString());

        return query;
    }

    public async Task<List<MessageIndex>> GetMessagesAsync(ulong authorId = default, ulong channelId = default, ulong guildId = default)
    {
        return await GetBaseQuery(authorId, channelId, guildId).ToListAsync();
    }

    public async Task<int> GetMessagesCountAsync(ulong authorId = default, ulong channelId = default, ulong guildId = default)
        => await GetBaseQuery(authorId, channelId, guildId).CountAsync();

    public async Task<MessageIndex?> FindMessageByIdAsync(ulong messageId)
    {
        return await GetBaseQuery()
            .FirstOrDefaultAsync(o => o.MessageId == messageId.ToString());
    }
}
