using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrillBot.Common.Managers.Counters;
using GrillBot.Database.Entity;
using GrillBot.Database.Models.Emotes;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class EmoteRepository : RepositoryBase
{
    public EmoteRepository(GrillBotContext context, CounterManager counter) : base(context, counter)
    {
    }

    public async Task<List<EmoteStatItem>> GetEmoteStatisticsDataAsync(IQueryableModel<EmoteStatisticItem> model,
        IEnumerable<string> emoteIds, bool unsupported)
    {
        using (Counter.Create("Database"))
        {
            var query = CreateQuery(model, true);
            query = unsupported ? query.Where(o => !emoteIds.Contains(o.EmoteId)) : query.Where(o => emoteIds.Contains(o.EmoteId));

            var grouped = query
                .GroupBy(o => o.EmoteId)
                .Select(o => new EmoteStatItem()
                {
                    EmoteId = o.Key,
                    FirstOccurence = o.Min(x => x.FirstOccurence),
                    LastOccurence = o.Max(x => x.LastOccurence),
                    UseCount = o.Sum(x => x.UseCount),
                    UsedUsersCount = o.Count()
                });

            return await grouped.ToListAsync();
        }
    }

    public async Task<List<EmoteStatisticItem>> FindStatisticsByEmoteIdAsync(string emoteId)
    {
        using (Counter.Create("Database"))
        {
            return await Context.Emotes
                .Where(o => o.EmoteId == emoteId)
                .ToListAsync();
        }
    }
}
