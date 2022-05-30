using GrillBot.Common.Managers.Counters;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrillBot.Database.Services.Repository;

public class ChannelRepository : RepositoryBase
{
    public ChannelRepository(GrillBotContext dbContext, CounterManager counter) : base(dbContext, counter)
    {
    }

    private IQueryable<GuildChannel> GetBaseQuery(bool includeDeleted, bool disableTracking)
    {
        var query = Context.Channels
            .Include(o => o.Users.Where(x => x.Count > 0 && (x.User!.User!.Flags & (long)UserFlags.NotUser) == 0))
            .AsQueryable();

        if (disableTracking)
            query = query.AsNoTracking();

        if (!includeDeleted)
            query = query.Where(o => (o.Flags & (long)ChannelFlags.Deleted) == 0);

        return query;
    }

    public async Task<GuildChannel?> FindChannelByIdAsync(ulong guildId, ulong channelId, bool disableTracking = false)
    {
        using (Counter.Create("Database"))
        {
            var query = GetBaseQuery(false, disableTracking);
            return await query.FirstOrDefaultAsync(o => o.GuildId == guildId.ToString() && o.ChannelId == channelId.ToString());
        }
    }

    public async Task<List<GuildChannel>> GetVisibleChannelsAsync(ulong guildId, List<string> channelIds, bool disableTracking = false)
    {
        using (Counter.Create("Database"))
        {
            var query = GetBaseQuery(false, disableTracking)
                .Where(o =>
                    o.GuildId == guildId.ToString() &&
                    (o.Flags & (long)ChannelFlags.StatsHidden) == 0 &&
                    channelIds.Contains(o.ChannelId) &&
                    o.Users.Count > 0
                );

            return await query.ToListAsync();
        }
    }
}
