using GrillBot.Common.Managers.Counters;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Cache.Services.Repository;

public class StatisticsRepository : RepositoryBase
{
    public StatisticsRepository(GrillBotCacheContext context, CounterManager counter) : base(context, counter)
    {
    }

    public async Task<Dictionary<string, int>> GetTableStatisticsAsync()
    {
        using (Counter.Create("Cache"))
        {
            return new Dictionary<string, int>
            {
                { nameof(Context.MessageIndex), await Context.MessageIndex.CountAsync() },
                { nameof(Context.DirectApiMessages), await Context.DirectApiMessages.CountAsync() },
                { nameof(Context.ProfilePictures), await Context.ProfilePictures.CountAsync() }
            };
        }
    }
}
