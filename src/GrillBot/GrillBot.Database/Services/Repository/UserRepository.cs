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

namespace GrillBot.Database.Services.Repository;

public class UserRepository : RepositoryBase
{
    public UserRepository(GrillBotContext context, CounterManager counter) : base(context, counter)
    {
    }

    public async Task<User?> FindUserByIdAsync(ulong id)
    {
        using (Counter.Create("Database"))
        {
            return await Context.Users
                .FirstOrDefaultAsync(o => o.Id == id.ToString());
        }
    }

    public async Task<User?> FindUserAsync(IUser user)
    {
        using (Counter.Create("Database"))
        {
            var entity = await Context.Users
                .FirstOrDefaultAsync(o => o.Id == user.Id.ToString());

            if (entity == null)
                return null;

            entity.Update(user);
            return entity;
        }
    }

    public async Task<User> GetOrCreateUserAsync(IUser user)
    {
        using (Counter.Create("Database"))
        {
            var entity = await Context.Users
                .FirstOrDefaultAsync(o => o.Id == user.Id.ToString());

            if (entity != null)
            {
                entity.Update(user);
                return entity;
            }

            entity = User.FromDiscord(user);
            if (!Context.IsEntityTracked<User>(entry => entry.Entity.Id == entity.Id)) await Context.AddAsync(entity);

            return entity;
        }
    }

    public async Task<List<User>> GetOnlineUsersAsync()
    {
        using (Counter.Create("Database"))
        {
            return await Context.Users
                .Where(o => (o.Flags & (int)UserFlags.PublicAdminOnline) != 0 || (o.Flags & (int)UserFlags.WebAdminOnline) != 0)
                .ToListAsync();
        }
    }

    public async Task<PaginatedResponse<User>> GetUsersListAsync(IQueryableModel<User> model, PaginatedParams pagination)
    {
        using (Counter.Create("Database"))
        {
            var query = CreateQuery(model, true);
            return await PaginatedResponse<User>.CreateWithEntityAsync(query, pagination);
        }
    }

    public async Task<User?> FindUserWithDetailsByIdAsync(ulong id)
    {
        using (Counter.Create("Database"))
        {
            var query = Context.Users.AsSplitQuery().AsNoTracking()
                .Include(o => o.Guilds).ThenInclude(o => o.Guild)
                .Include(o => o.Guilds).ThenInclude(o => o.UsedInvite.Creator!.User) // TODO UsedInvite is nullable, but ? cannot be used in lambda queries.
                .Include(o => o.Guilds).ThenInclude(o => o.CreatedInvites.Where(x => x.UsedUsers.Count > 0))
                .Include(o => o.Guilds).ThenInclude(o => o.Channels.Where(x => x.Count > 0)).ThenInclude(o => o.Channel)
                .Include(o => o.Guilds).ThenInclude(o => o.EmoteStatistics.Where(x => x.UseCount > 0))
                .Include(o => o.Guilds).ThenInclude(o => o.Unverify.UnverifyLog); // TODO Unverify is nullable, but ? cannot be used in lambda queries.

            return await query.FirstOrDefaultAsync(o => o.Id == id.ToString());
        }
    }

    public async Task<List<User>> GetUsersWithTodayBirthday()
    {
        using (Counter.Create("Database"))
        {
            var today = DateTime.Now;

            return await Context.Users.AsNoTracking()
                .Where(o => o.Birthday != null && o.Birthday.Value.Month == today.Month && o.Birthday.Value.Day == today.Day)
                .ToListAsync();
        }
    }
}
