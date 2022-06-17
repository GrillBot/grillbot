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

    public async Task<GuildChannel?> FindChannelByIdAsync(ulong channelId, ulong? guildId = null, bool disableTracking = false,
        bool includeUsers = false)
    {
        using (Counter.Create("Database"))
        {
            var query = GetBaseQuery(false, disableTracking, includeUsers);
            if (guildId != null)
                query = query.Where(o => o.GuildId == guildId.ToString());

            return await query.FirstOrDefaultAsync(o => o.ChannelId == channelId.ToString());
        }
    }

    public async Task<List<GuildChannel>> FindChannelsByIdAsync(ulong channelId, bool disableTracking = false, bool includeUsers = false)
    {
        using (Counter.Create("Database"))
        {
            var query = GetBaseQuery(false, disableTracking, includeUsers);

            return await query
                .Where(o => o.ChannelId == channelId.ToString())
                .ToListAsync();
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

    public async Task<List<GuildChannel>> GetAllChannelsAsync(List<string> guildIds, bool ignoreThreads, bool disableTracking = false)
    {
        using (Counter.Create("Database"))
        {
            var query = GetBaseQuery(false, disableTracking, false)
                .Where(o => guildIds.Contains(o.GuildId));

            if (ignoreThreads)
                query = query.Where(o => !new[] { ChannelType.NewsThread, ChannelType.PrivateThread, ChannelType.PublicThread }.Contains(o.ChannelType));

            return await query.ToListAsync();
        }
    }

    public async Task<GuildChannel> GetOrCreateChannelAsync(IGuildChannel channel)
    {
        using (Counter.Create("Database"))
        {
            var entity = await GetBaseQuery(true, false, false)
                .FirstOrDefaultAsync(o => o.GuildId == channel.GuildId.ToString() && o.ChannelId == channel.Id.ToString());

            if (entity != null)
                return entity;

            var guildEntity = await Context.Guilds.FirstOrDefaultAsync(o => o.Id == channel.GuildId.ToString()) ?? Guild.FromDiscord(channel.Guild);
            if (!Context.IsEntityTracked<Guild>(entry => entry.Entity.Id == guildEntity.Id)) await Context.AddAsync(guildEntity);

            entity = GuildChannel.FromDiscord(channel.Guild, channel, channel.GetChannelType() ?? ChannelType.DM);
            entity.Guild = guildEntity;
            if (!Context.IsEntityTracked<GuildChannel>(entry => entry.Entity.ChannelId == entity.ChannelId && entry.Entity.GuildId == entity.GuildId))
                await Context.AddAsync(entity);

            return entity;
        }
    }

    public async Task<long> GetMessagesCountOfUserAsync(IGuildUser user)
    {
        using (Counter.Create("Database"))
        {
            var query = Context.UserChannels.AsNoTracking()
                .Where(o =>
                    o.Count > 0 &&
                    o.GuildId == user.GuildId.ToString() &&
                    o.UserId == user.Id.ToString() &&
                    (o.Channel!.Flags & (long)ChannelFlags.StatsHidden) == 0 &&
                    (o.Channel!.Flags & (long)ChannelFlags.Deleted) == 0
                );

            return await query.SumAsync(o => o.Count);
        }
    }

    public async Task<(GuildUserChannel? lastActive, GuildUserChannel? mostActive)> GetTopChannelsOfUserAsync(IGuildUser user)
    {
        using (Counter.Create("Database"))
        {
            var baseQuery = Context.UserChannels.AsNoTracking()
                .Where(o =>
                    o.GuildId == user.GuildId.ToString() &&
                    (o.Channel!.Flags & (long)ChannelFlags.StatsHidden) == 0 &&
                    o.Channel!.ChannelType == ChannelType.Text &&
                    o.Count > 0 &&
                    (o.Channel!.Flags & (long)ChannelFlags.Deleted) == 0
                );

            var lastActive = await baseQuery.OrderByDescending(o => o.LastMessageAt).FirstOrDefaultAsync();
            var mostActive = await baseQuery.OrderByDescending(o => o.Count).FirstOrDefaultAsync();

            return (lastActive, mostActive);
        }
    }
}
