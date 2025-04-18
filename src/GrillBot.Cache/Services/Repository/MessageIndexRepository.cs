using GrillBot.Cache.Entity;
using GrillBot.Core.Database.Repository;
using GrillBot.Core.Managers.Performance;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Cache.Services.Repository;

public class MessageIndexRepository : SubRepositoryBase<GrillBotCacheContext>
{
    public MessageIndexRepository(GrillBotCacheContext context, ICounterManager counter) : base(context, counter)
    {
    }

    private IQueryable<MessageIndex> GetBaseQuery(ulong authorId = default, ulong channelId = default, ulong guildId = default)
    {
        var query = DbContext.MessageIndex.AsQueryable();

        if (authorId != default) query = query.Where(o => o.AuthorId == authorId.ToString());
        if (channelId != default) query = query.Where(o => o.ChannelId == channelId.ToString());
        if (guildId != default) query = query.Where(o => o.GuildId == guildId.ToString());

        return query;
    }

    public async Task<int> GetMessagesCountAsync(ulong authorId = default, ulong channelId = default, ulong guildId = default)
    {
        using (CreateCounter())
        {
            return await GetBaseQuery(authorId, channelId, guildId).CountAsync();
        }
    }

    public void DeleteAllIndexes()
    {
        using (CreateCounter())
        {
            DbContext.MessageIndex.ExecuteDelete();
        }
    }
}
