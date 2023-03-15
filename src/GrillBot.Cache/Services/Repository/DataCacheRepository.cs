using GrillBot.Cache.Entity;
using GrillBot.Core.Database.Repository;
using GrillBot.Core.Managers.Performance;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GrillBot.Cache.Services.Repository;

public class DataCacheRepository : RepositoryBase<GrillBotCacheContext>
{
    public DataCacheRepository(GrillBotCacheContext context, ICounterManager counter) : base(context, counter)
    {
    }

    public async Task<int> DeleteExpiredAsync()
    {
        using (CreateCounter())
        {
            return await Context.Database.ExecuteSqlRawAsync("DELETE FROM public.\"DataCache\" WHERE \"ValidTo\" < @now", new NpgsqlParameter("@now", DateTime.Now));
        }
    }

    public async Task<DataCacheItem?> FindItemAsync(string key, bool disableTracking = false, bool onlyValid = true)
    {
        using (CreateCounter())
        {
            var query = Context.DataCache.Where(o => o.Key == key);
            if (onlyValid)
                query = query.Where(o => o.ValidTo <= DateTime.Now);
            if (disableTracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync();
        }
    }
}
