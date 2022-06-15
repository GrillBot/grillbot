using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrillBot.Common.Managers.Counters;
using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class SearchingRepository : RepositoryBase
{
    public SearchingRepository(GrillBotContext context, CounterManager counter) : base(context, counter)
    {
    }

    public async Task<SearchItem?> FindSearchItemByIdAsync(long id)
    {
        using (Counter.Create("Database"))
        {
            return await Context.SearchItems
                .FirstOrDefaultAsync(o => o.Id == id);
        }
    }

    public async Task<List<SearchItem>> FindSearchesByIdsAsync(IEnumerable<long> ids)
    {
        using (Counter.Create("Database"))
        {
            return await Context.SearchItems
                .Where(o => ids.Contains(o.Id))
                .ToListAsync();
        }
    }

    public async Task<List<SearchItem>> FindSearchesAsync(IQueryableModel<SearchItem> model)
    {
        using (Counter.Create("Database"))
        {
            var query = CreateQuery(model);
            return await query.ToListAsync();
        }
    }
}
