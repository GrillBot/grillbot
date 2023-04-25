using System;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using GrillBot.Core.Database;
using GrillBot.Core.Database.Repository;
using GrillBot.Core.Managers.Performance;
using GrillBot.Core.Models.Pagination;
using GrillBot.Database.Enums.Internal;

namespace GrillBot.Database.Services.Repository;

public class ChannelRepository : RepositoryBase<GrillBotContext>
{
    public ChannelRepository(GrillBotContext dbContext, ICounterManager counter) : base(dbContext, counter)
    {
    }

    private IQueryable<GuildChannel> GetBaseQuery(bool includeDeleted, bool disableTracking, ChannelsIncludeUsersMode includeUsersMode)
    {
        var query = Context.Channels
            .Include(o => o.Guild)
            .AsQueryable();

        query = includeUsersMode switch
        {
            ChannelsIncludeUsersMode.IncludeAll => query.Include(o => o.Users).ThenInclude(o => o.User!.User),
            ChannelsIncludeUsersMode.IncludeExceptInactive =>
                query.Include(o => o.Users.Where(x => x.Count > 0 && (x.User!.User!.Flags & (long)UserFlags.NotUser) == 0)).ThenInclude(o => o.User!.User),
            _ => query
        };

        if (disableTracking)
            query = query.AsNoTracking();

        if (!includeDeleted)
            query = query.Where(o => (o.Flags & (long)ChannelFlag.Deleted) == 0);

        return query;
    }

    public async Task<GuildChannel?> FindChannelByIdAsync(ulong channelId, ulong? guildId = null, bool disableTracking = false,
        ChannelsIncludeUsersMode includeUsersMode = ChannelsIncludeUsersMode.None, bool includeParent = false, bool includeDeleted = false)
    {
        using (CreateCounter())
        {
            var query = GetBaseQuery(includeDeleted, disableTracking, includeUsersMode);
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
        using (CreateCounter())
        {
            var query = GetBaseQuery(false, disableTracking, ChannelsIncludeUsersMode.IncludeExceptInactive)
                .Where(o =>
                    o.GuildId == guildId.ToString() &&
                    channelIds.Contains(o.ChannelId) &&
                    o.Users.Count > 0 &&
                    o.ChannelType != ChannelType.Category
                );

            if (!showInvisible)
                query = query.Where(o => (o.Flags & (long)ChannelFlag.StatsHidden) == 0);

            return await query.ToListAsync();
        }
    }

    public async Task<List<GuildChannel>> GetAllChannelsAsync(bool disableTracking = false, bool includeDeleted = true, bool includeUsers = false, List<ChannelType>? channelTypes = null)
    {
        using (CreateCounter())
        {
            var usersIncludeMode = includeUsers ? ChannelsIncludeUsersMode.IncludeAll : ChannelsIncludeUsersMode.None;
            var query = GetBaseQuery(includeDeleted, disableTracking, usersIncludeMode);
            if (channelTypes?.Count > 0)
                query = query.Where(o => channelTypes.Contains(o.ChannelType));

            return await query.ToListAsync();
        }
    }

    public async Task<List<GuildChannel>> GetAllChannelsAsync(List<string> guildIds, bool ignoreThreads, bool disableTracking = false, ChannelFlag filterFlag = ChannelFlag.None)
    {
        using (CreateCounter())
        {
            var query = GetBaseQuery(false, disableTracking, ChannelsIncludeUsersMode.None)
                .Where(o => guildIds.Contains(o.GuildId));

            if (ignoreThreads)
                query = query.Where(o => !new[] { ChannelType.NewsThread, ChannelType.PrivateThread, ChannelType.PublicThread }.Contains(o.ChannelType));
            if (filterFlag > ChannelFlag.None)
                query = query.Where(o => (o.Flags & (long)filterFlag) != 0);

            return await query.ToListAsync();
        }
    }

    public async Task<GuildChannel> GetOrCreateChannelAsync(IGuildChannel channel, ChannelsIncludeUsersMode includeUsersMode = ChannelsIncludeUsersMode.None)
    {
        using (CreateCounter())
        {
            var entity = await GetBaseQuery(true, false, includeUsersMode)
                .FirstOrDefaultAsync(o => o.GuildId == channel.GuildId.ToString() && o.ChannelId == channel.Id.ToString());

            if (entity != null)
            {
                entity.Update(channel);
                return entity;
            }

            entity = GuildChannel.FromDiscord(channel, channel.GetChannelType() ?? ChannelType.DM);
            await Context.AddAsync(entity);

            return entity;
        }
    }

    public async Task<long> GetMessagesCountOfUserAsync(IGuildUser user)
    {
        using (CreateCounter())
        {
            var query = Context.UserChannels.AsNoTracking()
                .Where(o =>
                    o.Count > 0 &&
                    o.GuildId == user.GuildId.ToString() &&
                    o.UserId == user.Id.ToString() &&
                    (o.Channel.Flags & (long)ChannelFlag.Deleted) == 0 &&
                    o.Channel.ChannelType != ChannelType.Category
                );

            return await query.SumAsync(o => o.Count);
        }
    }

    public async Task<(GuildUserChannel? lastActive, GuildUserChannel? mostActive)> GetTopChannelStatsOfUserAsync(IGuildUser user)
    {
        using (CreateCounter())
        {
            var baseQuery = Context.UserChannels.AsNoTracking()
                .Where(o =>
                    o.GuildId == user.GuildId.ToString() &&
                    (o.Channel.Flags & (long)ChannelFlag.StatsHidden) == 0 &&
                    o.Count > 0 &&
                    (o.Channel.Flags & (long)ChannelFlag.Deleted) == 0 &&
                    o.UserId == user.Id.ToString()
                );

            var lastActive = await baseQuery.OrderByDescending(o => o.LastMessageAt).FirstOrDefaultAsync();
            var mostActive = await baseQuery.OrderByDescending(o => o.Count).FirstOrDefaultAsync();

            return (lastActive, mostActive);
        }
    }

    public async Task<List<GuildChannel>> GetChildChannelsAsync(ulong parentChannelId, ulong? guildId = null)
    {
        using (CreateCounter())
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
        using (CreateCounter())
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
        using (CreateCounter())
        {
            var query = CreateQuery(model, true);
            return await PaginatedResponse<GuildChannel>.CreateWithEntityAsync(query, pagination);
        }
    }

    public async Task<PaginatedResponse<GuildUserChannel>> GetUserChannelListAsync(ulong channelId, PaginatedParams pagination)
    {
        using (CreateCounter())
        {
            var query = Context.UserChannels.AsNoTracking()
                .Include(o => o.User!.User)
                .OrderByDescending(o => o.Count)
                .ThenByDescending(o => o.LastMessageAt)
                .Where(o => o.ChannelId == channelId.ToString() && o.Count > 0);

            return await PaginatedResponse<GuildUserChannel>.CreateWithEntityAsync(query, pagination);
        }
    }

    public async Task<Dictionary<string, (long count, DateTime firstMessageAt, DateTime lastMessageAt)>> GetAvailableStatsAsync(IGuild guild, IEnumerable<string> availableChannelIds,
        bool showInvisible = false)
    {
        using (CreateCounter())
        {
            var query = Context.UserChannels.AsNoTracking()
                .Where(o => o.Count > 0 && o.GuildId == guild.Id.ToString() && availableChannelIds.Contains(o.ChannelId));

            if (!showInvisible)
                query = query.Where(o => (o.Channel.Flags & (long)ChannelFlag.StatsHidden) == 0);

            var groupQuery = query.GroupBy(o => o.ChannelId)
                .Select(o => new
                {
                    ChannelId = o.Key,
                    Count = o.Sum(x => x.Count),
                    LastMessageAt = o.Max(x => x.LastMessageAt),
                    FirstMessageAt = o.Min(x => x.FirstMessageAt)
                });

            return await groupQuery.ToDictionaryAsync(o => o.ChannelId, o => (o.Count, o.FirstMessageAt, o.LastMessageAt));
        }
    }

    public async Task<bool> IsChannelEphemeralAsync(IGuild guild, IChannel channel)
    {
        using (CreateCounter())
        {
            return await Context.Channels.AsNoTracking()
                .Where(o => (o.Flags & (long)ChannelFlag.Deleted) == 0)
                .AnyAsync(o => o.GuildId == guild.Id.ToString() && o.ChannelId == channel.Id.ToString() && (o.Flags & (long)ChannelFlag.EphemeralCommands) != 0);
        }
    }

    public async Task<GuildUserChannel?> FindUserChannelAsync(IGuildChannel channel, IUser user)
    {
        using (CreateCounter())
        {
            return await Context.UserChannels
                .FirstOrDefaultAsync(o => o.GuildId == channel.GuildId.ToString() && o.ChannelId == channel.Id.ToString() && o.UserId == user.Id.ToString());
        }
    }

    public async Task<GuildUserChannel> GetOrCreateUserChannelAsync(IGuildChannel channel, IUser user)
    {
        using (CreateCounter())
        {
            var userChannel = await FindUserChannelAsync(channel, user);
            if (userChannel != null) return userChannel;

            userChannel = new GuildUserChannel
            {
                ChannelId = channel.Id.ToString(),
                GuildId = channel.GuildId.ToString(),
                UserId = user.Id.ToString(),
                FirstMessageAt = DateTime.Now,
                Count = 0
            };

            await Context.AddAsync(userChannel);
            return userChannel;
        }
    }

    public async Task<List<GuildUserChannel>> GetUserStatisticsAsync(IGuildChannel channel, bool excludeThreads)
    {
        using (CreateCounter())
        {
            var query = Context.UserChannels.AsNoTracking()
                .Include(o => o.Channel.ParentChannel)
                .Where(o =>
                    o.GuildId == channel.GuildId.ToString() &&
                    o.Count > 0 &&
                    (o.Channel.Flags & (long)ChannelFlag.Deleted) == 0
                );

            query = excludeThreads
                ? query.Where(o => o.ChannelId == channel.Id.ToString())
                : query.Where(o => o.ChannelId == channel.Id.ToString() || o.Channel.ParentChannelId == channel.Id.ToString());
            return await query.ToListAsync();
        }
    }

    public async Task<bool> HaveChannelFlagsAsync(IGuildChannel channel, ChannelFlag flag)
    {
        using (CreateCounter())
        {
            return await Context.Channels.AsNoTracking()
                .AnyAsync(o => o.GuildId == channel.GuildId.ToString() && o.ChannelId == channel.Id.ToString() && (o.Flags & (long)flag) != 0);
        }
    }
}
