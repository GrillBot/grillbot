using GrillBot.Common.Managers.Counters;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;

namespace GrillBot.Database.Services.Repository;

public class ChannelRepository : RepositoryBase
{
    public ChannelRepository(GrillBotContext dbContext, CounterManager counter) : base(dbContext, counter)
    {
    }

    private IQueryable<GuildChannel> GetBaseQuery(bool includeDeleted, bool disableTracking, bool includeUsers)
    {
        var query = Context.Channels.AsQueryable();

        if (includeUsers)
            query = query.Include(o => o.Users.Where(x => x.Count > 0 && (x.User!.User!.Flags & (long)UserFlags.NotUser) == 0));

        if (disableTracking)
            query = query.AsNoTracking();

        if (!includeDeleted)
            query = query.Where(o => (o.Flags & (long)ChannelFlags.Deleted) == 0);

        return query
            .Where(o => o.ChannelType != ChannelType.Category);
    }

    public async Task<GuildChannel?> FindChannelByIdAsync(ulong guildId, ulong channelId, bool disableTracking = false)
    {
        using (Counter.Create("Database"))
        {
            var query = GetBaseQuery(false, disableTracking, true);
            return await query.FirstOrDefaultAsync(o => o.GuildId == guildId.ToString() && o.ChannelId == channelId.ToString());
        }
    }

    public async Task<List<GuildChannel>> GetVisibleChannelsAsync(ulong guildId, List<string> channelIds, bool disableTracking = false)
    {
        using (Counter.Create("Database"))
        {
            var query = GetBaseQuery(false, disableTracking, true)
                .Where(o =>
                    o.GuildId == guildId.ToString() &&
                    (o.Flags & (long)ChannelFlags.StatsHidden) == 0 &&
                    channelIds.Contains(o.ChannelId) &&
                    o.Users.Count > 0
                );

            return await query.ToListAsync();
        }
    }

    public async Task<List<GuildChannel>> GetAllChannelsAsync(List<string> guildIds, bool ignoreThreads, bool disableTracking = false,
        CancellationToken cancellationToken = default)
    {
        using (Counter.Create("Database"))
        {
            var query = GetBaseQuery(false, disableTracking, false)
                .Where(o => guildIds.Contains(o.GuildId));

            if (ignoreThreads)
                query = query.Where(o => !new[] { ChannelType.NewsThread, ChannelType.PrivateThread, ChannelType.PublicThread }.Contains(o.ChannelType));

            return await query.ToListAsync(cancellationToken);
        }
    }
}
