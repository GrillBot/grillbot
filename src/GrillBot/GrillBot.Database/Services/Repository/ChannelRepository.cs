using System;
using GrillBot.Common.Managers.Counters;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using GrillBot.Database.Models;

namespace GrillBot.Database.Services.Repository;

public class ChannelRepository : RepositoryBase
{
    public ChannelRepository(GrillBotContext dbContext, CounterManager counter) : base(dbContext, counter)
    {
    }

    private IQueryable<GuildChannel> GetBaseQuery(bool includeDeleted, bool disableTracking, bool includeUsers)
    {
        var query = Context.Channels
            .Include(o => o.Guild)
            .AsQueryable();

        if (includeUsers)
            query = query.Include(o => o.Users.Where(x => x.Count > 0 && (x.User!.User!.Flags & (long)UserFlags.NotUser) == 0)).ThenInclude(o => o.User!.User);

        if (disableTracking)
            query = query.AsNoTracking();

        if (!includeDeleted)
            query = query.Where(o => (o.Flags & (long)ChannelFlags.Deleted) == 0);

        return query;
    }

    public async Task<GuildChannel?> FindChannelByIdAsync(ulong channelId, ulong? guildId = null, bool disableTracking = false,
        bool includeUsers = false, bool includeParent = false, bool includeDeleted = false)
    {
        using (Counter.Create("Database"))
        {
            var query = GetBaseQuery(includeDeleted, disableTracking, includeUsers);
            if (guildId != null)
                query = query.Where(o => o.GuildId == guildId.ToString());
            if (includeParent)
                query = query.Include(o => o.ParentChannel);

            return await query.FirstOrDefaultAsync(o => o.ChannelId == channelId.ToString());
        }
    }

    public async Task<List<GuildChannel>> GetVisibleChannelsAsync(ulong guildId, List<string> channelIds, bool disableTracking = false,
        bool showInvisible = false)
    {
        using (Counter.Create("Database"))
        {
            var query = GetBaseQuery(false, disableTracking, true)
                .Where(o =>
                    o.GuildId == guildId.ToString() &&
                    channelIds.Contains(o.ChannelId) &&
                    o.Users.Count > 0 &&
                    o.ChannelType != ChannelType.Category
                );

            if (!showInvisible)
                query = query.Where(o => (o.Flags & (long)ChannelFlags.StatsHidden) == 0);

            return await query.ToListAsync();
        }
    }

    public async Task<List<GuildChannel>> GetAllChannelsAsync(bool disableTracking = false, bool includeDeleted = true)
    {
        using (Counter.Create("Database"))
        {
            return await GetBaseQuery(includeDeleted, disableTracking, false).ToListAsync();
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

    public async Task<GuildChannel> GetOrCreateChannelAsync(IGuildChannel channel, bool includeUsers = false)
    {
        using (Counter.Create("Database"))
        {
            var entity = await GetBaseQuery(true, false, includeUsers)
                .FirstOrDefaultAsync(o => o.GuildId == channel.GuildId.ToString() && o.ChannelId == channel.Id.ToString());

            if (entity != null)
                return entity;

            entity = GuildChannel.FromDiscord(channel, channel.GetChannelType() ?? ChannelType.DM);
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
                    (o.Channel.Flags & (long)ChannelFlags.StatsHidden) == 0 &&
                    (o.Channel.Flags & (long)ChannelFlags.Deleted) == 0 &&
                    o.Channel.ChannelType != ChannelType.Category
                );

            return await query.SumAsync(o => o.Count);
        }
    }

    public async Task<(GuildUserChannel? lastActive, GuildUserChannel? mostActive)> GetTopChannelStatsOfUserAsync(IGuildUser user)
    {
        using (Counter.Create("Database"))
        {
            var baseQuery = Context.UserChannels.AsNoTracking()
                .Where(o =>
                    o.GuildId == user.GuildId.ToString() &&
                    (o.Channel.Flags & (long)ChannelFlags.StatsHidden) == 0 &&
                    o.Channel.ChannelType == ChannelType.Text &&
                    o.Count > 0 &&
                    (o.Channel.Flags & (long)ChannelFlags.Deleted) == 0 &&
                    o.UserId == user.Id.ToString()
                );

            var lastActive = await baseQuery.OrderByDescending(o => o.LastMessageAt).FirstOrDefaultAsync();
            var mostActive = await baseQuery.OrderByDescending(o => o.Count).FirstOrDefaultAsync();

            return (lastActive, mostActive);
        }
    }

    public async Task<List<GuildChannel>> GetTopChannelsOfUserAsync(IGuildUser user, int take, bool disableTracking = false)
    {
        using (Counter.Create("Database"))
        {
            var query = Context.UserChannels
                .Where(o =>
                    o.Channel.ChannelType == ChannelType.Text &&
                    o.GuildId == user.GuildId.ToString() &&
                    o.UserId == user.Id.ToString() &&
                    o.Count > 0 &&
                    (o.Channel.Flags & (long)ChannelFlags.Deleted) == 0
                )
                .OrderByDescending(o => o.Count)
                .Select(o => o.Channel)
                .Take(take);

            if (disableTracking)
                query = query.AsNoTracking();

            return await query.ToListAsync();
        }
    }

    public async Task<List<GuildChannel>> GetChildChannelsAsync(ulong parentChannelId, ulong? guildId = null)
    {
        using (Counter.Create("Database"))
        {
            var query = Context.Channels
                .Where(o =>
                    new[] { ChannelType.NewsThread, ChannelType.PrivateThread, ChannelType.PublicThread }.Contains(o.ChannelType) &&
                    o.ParentChannelId == parentChannelId.ToString()
                );

            if (guildId != null)
                query = query.Where(o => o.GuildId == guildId.ToString());

            return await query.ToListAsync();
        }
    }

    public async Task<GuildChannel?> FindThreadAsync(IThreadChannel thread)
    {
        using (Counter.Create("Database"))
        {
            var query = Context.Channels
                .Where(o =>
                    o.GuildId == thread.GuildId.ToString() &&
                    new[] { ChannelType.NewsThread, ChannelType.PrivateThread, ChannelType.PublicThread }.Contains(o.ChannelType) &&
                    o.ChannelId == thread.Id.ToString()
                );

            if (thread.CategoryId != null)
                query = query.Where(o => o.ParentChannelId == thread.CategoryId.ToString());

            return await query.FirstOrDefaultAsync();
        }
    }

    public async Task<PaginatedResponse<GuildChannel>> GetChannelListAsync(IQueryableModel<GuildChannel> model, PaginatedParams pagination)
    {
        using (Counter.Create("Database"))
        {
            var query = CreateQuery(model, true);
            return await PaginatedResponse<GuildChannel>.CreateWithEntityAsync(query, pagination);
        }
    }

    public async Task<PaginatedResponse<GuildUserChannel>> GetUserChannelListAsync(ulong channelId, PaginatedParams pagination)
    {
        using (Counter.Create("Database"))
        {
            var query = Context.UserChannels.AsNoTracking()
                .Include(o => o.User!.User)
                .OrderByDescending(o => o.Count)
                .ThenByDescending(o => o.LastMessageAt)
                .Where(o => o.ChannelId == channelId.ToString() && o.Count > 0);

            return await PaginatedResponse<GuildUserChannel>.CreateWithEntityAsync(query, pagination);
        }
    }

    public async Task<List<(string channelId, long count, DateTime firstMessageAt, DateTime lastMessageAt)>> GetAvailableStatsAsync(IGuild guild, IEnumerable<string> availableChannelIds)
    {
        using (Counter.Create("Database"))
        {
            var query = Context.UserChannels.AsNoTracking()
                .Where(o => o.Count > 0 && o.GuildId == guild.Id.ToString() && availableChannelIds.Contains(o.ChannelId))
                .GroupBy(o => o.ChannelId)
                .Select(o => new
                {
                    ChannelId = o.Key,
                    Count = o.Sum(x => x.Count),
                    LastMessageAt = o.Max(x => x.LastMessageAt),
                    FirstMessageAt = o.Min(x => x.FirstMessageAt)
                });

            var data = await query.ToListAsync();
            return data.ConvertAll(o => (o.ChannelId, o.Count, o.FirstMessageAt, o.LastMessageAt));
        }
    }
}
