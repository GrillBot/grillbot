using System;
using System.Collections.Generic;
using System.Linq;
using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Discord;
using GrillBot.Core.Database;
using GrillBot.Core.Database.Repository;
using GrillBot.Core.Managers.Performance;
using GrillBot.Core.Models.Pagination;
using GrillBot.Database.Enums;
using GrillBot.Database.Enums.Internal;
using System.Threading;

namespace GrillBot.Database.Services.Repository;

public class UserRepository : SubRepositoryBase<GrillBotContext>
{
    public UserRepository(GrillBotContext context, ICounterManager counter) : base(context, counter)
    {
    }

    public async Task<User?> FindUserAsync(IUser user, bool disableTracking = false)
    {
        using (CreateCounter())
        {
            var query = DbContext.Users.AsQueryable();
            if (disableTracking)
                query = query.AsNoTracking();

            var entity = await query
                .FirstOrDefaultAsync(o => o.Id == user.Id.ToString());

            if (entity == null)
                return null;

            if (!disableTracking)
                entity.Update(user);
            return entity;
        }
    }

    public async Task<User> GetOrCreateUserAsync(IUser user)
    {
        using (CreateCounter())
        {
            var entity = await FindUserAsync(user);
            if (entity != null)
                return entity;

            entity = User.FromDiscord(user);
            await DbContext.AddAsync(entity);

            return entity;
        }
    }

    public async Task<List<User>> GetOnlineUsersAsync()
    {
        using (CreateCounter())
        {
            return await DbContext.Users
                .Where(o => (o.Flags & (int)UserFlags.PublicAdminOnline) != 0 || (o.Flags & (int)UserFlags.WebAdminOnline) != 0)
                .ToListAsync();
        }
    }

    public async Task<PaginatedResponse<User>> GetUsersListAsync(IQueryableModel<User> model, PaginatedParams pagination)
    {
        using (CreateCounter())
        {
            var query = CreateQuery(model, true);
            return await PaginatedResponse<User>.CreateWithEntityAsync(query, pagination);
        }
    }

    public async Task<User?> FindUserByIdAsync(ulong id, UserIncludeOptions includeOptions = UserIncludeOptions.None, bool disableTracking = false, CancellationToken cancellationToken = default)
    {
        using (CreateCounter())
        {
            var query = DbContext.Users.AsQueryable();
            if (includeOptions.HasFlag(UserIncludeOptions.Guilds))
                query = query.Include(o => o.Guilds).ThenInclude(o => o.Guild);
            if (includeOptions.HasFlag(UserIncludeOptions.Channels))
                query = query.Include(o => o.Guilds).ThenInclude(o => o.Channels.Where(x => x.Count > 0)).ThenInclude(o => o.Channel);
            if (includeOptions.HasFlag(UserIncludeOptions.Unverify))
                query = query.Include(o => o.Guilds).ThenInclude(o => o.Unverify!.UnverifyLog);
            if (disableTracking)
                query = query.AsNoTracking();
            if (includeOptions != UserIncludeOptions.None)
                query = query.AsSplitQuery();

            return await query.FirstOrDefaultAsync(o => o.Id == id.ToString(), cancellationToken);
        }
    }

    private IQueryable<User> GetUsersWithTodayBirthdayQuery()
    {
        var today = DateTime.Now;

        return DbContext.Users.AsNoTracking()
            .Where(o => o.Birthday != null && o.Birthday.Value.Month == today.Month && o.Birthday.Value.Day == today.Day)
            .OrderBy(o => o.Id);
    }

    public async Task<bool> HaveSomeoneBirthdayTodayAsync()
    {
        using (CreateCounter())
        {
            return await GetUsersWithTodayBirthdayQuery().AnyAsync();
        }
    }

    public async Task<List<User>> GetUsersWithTodayBirthday()
    {
        using (CreateCounter())
        {
            return await GetUsersWithTodayBirthdayQuery().ToListAsync();
        }
    }

    public async Task<List<User>> GetFullListOfUsers(bool? bots, IEnumerable<string>? mutualGuildIds, ulong? guildId)
    {
        using (CreateCounter())
        {
            var query = DbContext.Users.AsNoTracking();

            switch (bots)
            {
                case true:
                    query = query.Where(o => (o.Flags & (int)UserFlags.NotUser) != 0);
                    break;
                case false:
                    query = query.Where(o => (o.Flags & (int)UserFlags.NotUser) == 0);
                    break;
            }

            if (mutualGuildIds != null)
                query = query.Where(o => o.Guilds.Any(x => mutualGuildIds.Contains(x.GuildId)));
            if (guildId != null)
                query = query.Where(o => o.Guilds.Any(x => x.GuildId == guildId.Value.ToString()));

            return await query.ToListAsync();
        }
    }

    public async Task<List<User>> GetUsersByIdsAsync(List<string> userIds)
    {
        using (CreateCounter())
        {
            if (!userIds.Any())
                return new List<User>();

            return await DbContext.Users.AsNoTracking()
                .Where(o => userIds.Contains(o.Id))
                .ToListAsync();
        }
    }

    public async Task<List<User>> GetAdministratorsAsync()
    {
        using (CreateCounter())
        {
            return await DbContext.Users.AsNoTracking()
                .Where(o => (o.Flags & (int)UserFlags.BotAdmin) != 0)
                .ToListAsync();
        }
    }
}
