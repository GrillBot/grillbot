using System.Threading.Tasks;
using Discord;
using GrillBot.Common.Managers.Counters;
using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class InviteRepository : RepositoryBase
{
    public InviteRepository(GrillBotContext context, CounterManager counter) : base(context, counter)
    {
    }

    public async Task<Invite?> FindInviteByCodeAsync(IGuild guild, string code)
    {
        using (Counter.Create("Database"))
        {
            return await Context.Invites
                .FirstOrDefaultAsync(o => o.GuildId == guild.ToString() && o.Code == code);
        }
    }
}
