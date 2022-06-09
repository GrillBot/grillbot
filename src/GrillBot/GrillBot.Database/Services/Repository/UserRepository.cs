using GrillBot.Common.Managers.Counters;
using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Discord;

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
            await Context.AddAsync(entity);

            return entity;
        }
    }
}
