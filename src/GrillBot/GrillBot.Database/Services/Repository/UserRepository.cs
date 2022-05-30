using GrillBot.Common.Managers.Counters;
using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

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
}
