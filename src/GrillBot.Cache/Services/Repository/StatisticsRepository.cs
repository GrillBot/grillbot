using GrillBot.Core.Database.Repository;
using GrillBot.Core.Managers.Performance;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Cache.Services.Repository;

public class StatisticsRepository : RepositoryBase<GrillBotCacheContext>
{
    public StatisticsRepository(GrillBotCacheContext context, ICounterManager counter) : base(context, counter)
    {
    }

    public async Task<Dictionary<string, int>> GetTableStatisticsAsync()
    {
        using (CreateCounter())
        {
            return new Dictionary<string, int>
            {
                { nameof(Context.MessageIndex), await Context.MessageIndex.CountAsync() },
                { nameof(Context.ProfilePictures), await Context.ProfilePictures.CountAsync() },
                { nameof(Context.InviteMetadata), await Context.InviteMetadata.CountAsync() },
                { nameof(Context.DataCache), await Context.DataCache.CountAsync() },
                { nameof(Context.EmoteSuggestions), await Context.EmoteSuggestions.CountAsync() }
            };
        }
    }
}
