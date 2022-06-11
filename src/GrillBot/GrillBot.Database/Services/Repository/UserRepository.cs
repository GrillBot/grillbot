using System.Collections.Generic;
using System.Linq;
using GrillBot.Common.Managers.Counters;
using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Discord;
using GrillBot.Database.Enums;

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

    public async Task<User> GetOrCreateUserAsync(IUser user)
    {
        using (Counter.Create("Database"))
        {
            var entity = await Context.Users
                .FirstOrDefaultAsync(o => o.Id == user.Id.ToString());

            if (entity != null)
                return entity;

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
}
