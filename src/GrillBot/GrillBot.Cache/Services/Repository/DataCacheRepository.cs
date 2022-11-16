using GrillBot.Cache.Entity;
using GrillBot.Common.Managers.Counters;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GrillBot.Cache.Services.Repository;

public class DataCacheRepository : RepositoryBase
{
    public DataCacheRepository(GrillBotCacheContext context, CounterManager counter) : base(context, counter)
    {
    }

    public async Task DeleteExpiredAsync()
    {
        if (Context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory") return;
    
        using (CreateCounter())
        {
            await Context.Database.ExecuteSqlRawAsync("DELETE FROM public.\"DataCache\" WHERE \"ValidTo\" < @now", new NpgsqlParameter("@now", DateTime.Now));
        }
    }

    public async Task<DataCacheItem?> FindItemAsync(string key, bool disableTracking = false)
    {
        using (CreateCounter())
        {
            var query = Context.DataCache.Where(o => o.Key == key);
            if (disableTracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync();
        }
    }
}
