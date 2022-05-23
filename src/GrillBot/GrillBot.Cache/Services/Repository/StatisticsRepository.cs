using Microsoft.EntityFrameworkCore;

namespace GrillBot.Cache.Services.Repository;

public class StatisticsRepository : RepositoryBase
{
    public StatisticsRepository(GrillBotCacheContext context) : base(context)
    {
    }

    public async Task<Dictionary<string, int>> GetTableStatisticsAsync(CancellationToken cancellationToken = default)
    {
        return new Dictionary<string, int>()
        {
            { nameof(Context.MessageIndex), await Context.MessageIndex.CountAsync(cancellationToken) },
            { nameof(Context.DirectApiMessages), await Context.DirectApiMessages.CountAsync(cancellationToken) },
            { nameof(Context.ProfilePictures), await Context.ProfilePictures.CountAsync(cancellationToken) }
        };
    }
}
