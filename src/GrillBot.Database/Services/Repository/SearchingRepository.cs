using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrillBot.Common.Managers.Counters;
using GrillBot.Database.Entity;
using GrillBot.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class SearchingRepository : RepositoryBase
{
    public SearchingRepository(GrillBotContext context, CounterManager counter) : base(context, counter)
    {
    }

    public async Task<SearchItem?> FindSearchItemByIdAsync(long id)
    {
        using (CreateCounter())
        {
            return await Context.SearchItems
                .FirstOrDefaultAsync(o => o.Id == id);
        }
    }

    public async Task<List<SearchItem>> FindSearchesByIdsAsync(IEnumerable<long> ids)
    {
        using (CreateCounter())
        {
            return await Context.SearchItems
                .Where(o => ids.Contains(o.Id))
                .ToListAsync();
        }
    }

    private IQueryable<SearchItem> GetSearchesQuery(IQueryableModel<SearchItem> model, List<string> mutualGuilds)
    {
        var query = CreateQuery(model);
        if (mutualGuilds.Count > 0)
            query = query.Where(o => mutualGuilds.Contains(o.GuildId));
        return query;
    }

    public async Task<PaginatedResponse<SearchItem>> FindSearchesAsync(IQueryableModel<SearchItem> model, List<string> mutualGuilds, PaginatedParams parameters)
    {
        using (CreateCounter())
        {
            var query = GetSearchesQuery(model, mutualGuilds);
            return await PaginatedResponse<SearchItem>.CreateWithEntityAsync(query, parameters);
        }
    }

    public async Task<int> GetSearchesCountAsync(IQueryableModel<SearchItem> model, List<string> mutualGuilds)
    {
        using (CreateCounter())
        {
            return await GetSearchesQuery(model, mutualGuilds).CountAsync();
        }
    }
}
