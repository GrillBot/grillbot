using System;
using System.Collections.Generic;
using System.Linq;
using GrillBot.Common.Managers.Counters;
using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Discord;
using GrillBot.Database.Enums;
using GrillBot.Database.Models;
using Npgsql;

namespace GrillBot.Database.Services.Repository;

public class UserRepository : RepositoryBase
{
    public UserRepository(GrillBotContext context, CounterManager counter) : base(context, counter)
    {
    }

    public async Task<User?> FindUserByIdAsync(ulong id)
    {
        using (CreateCounter())
        {
            return await Context.Users
                .FirstOrDefaultAsync(o => o.Id == id.ToString());
        }
    }

    public async Task<User?> FindUserAsync(IUser user, bool disableTracking = false)
    {
        using (CreateCounter())
        {
            var query = Context.Users.AsQueryable();
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
            await Context.AddAsync(entity);

            return entity;
        }
    }

    public async Task<List<User>> GetOnlineUsersAsync()
    {
        using (CreateCounter())
        {
            return await Context.Users
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

    public async Task<User?> FindUserWithDetailsByIdAsync(ulong id)
    {
        using (CreateCounter())
        {
            var query = Context.Users.AsSplitQuery().AsNoTracking()
                .Include(o => o.Guilds).ThenInclude(o => o.Guild)
                .Include(o => o.Guilds).ThenInclude(o => o.UsedInvite!.Creator!.User)
                .Include(o => o.Guilds).ThenInclude(o => o.CreatedInvites.Where(x => x.UsedUsers.Count > 0))
                .Include(o => o.Guilds).ThenInclude(o => o.Channels.Where(x => x.Count > 0)).ThenInclude(o => o.Channel)
                .Include(o => o.Guilds).ThenInclude(o => o.EmoteStatistics.Where(x => x.UseCount > 0))
                .Include(o => o.Guilds).ThenInclude(o => o.Unverify!.UnverifyLog);

            return await query.FirstOrDefaultAsync(o => o.Id == id.ToString());
        }
    }

    public async Task<List<User>> GetUsersWithTodayBirthday()
    {
        using (CreateCounter())
        {
            var today = DateTime.Now;

            return await Context.Users.AsNoTracking()
                .Where(o => o.Birthday != null && o.Birthday.Value.Month == today.Month && o.Birthday.Value.Day == today.Day)
                .ToListAsync();
        }
    }

    public async Task<List<User>> FindAllUsersExceptBots()
    {
        using (CreateCounter())
        {
            return await Context.Users.AsNoTracking()
                .Where(o => (o.Flags & (int)UserFlags.NotUser) == 0)
                .ToListAsync();
        }
    }

    public async Task<List<User>> GetFullListOfUsers(bool? bots, IEnumerable<string>? mutualGuildIds)
    {
        using (CreateCounter())
        {
            var query = Context.Users.AsNoTracking();

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

            return await query
                .OrderBy(o => o.Username)
                .ThenBy(o => o.Discriminator)
                .ToListAsync();
        }
    }

    public async Task UpdateStatusAsync(ulong userId, UserStatus status)
    {
        using (CreateCounter())
        {
            await Context.Database.ExecuteSqlRawAsync(
                "UPDATE public.\"Users\" SET \"Status\"=@status WHERE \"Id\"=@userId",
                new NpgsqlParameter("@status", (int)status),
                new NpgsqlParameter("@userId", userId.ToString())
            );
        }
    }
}
