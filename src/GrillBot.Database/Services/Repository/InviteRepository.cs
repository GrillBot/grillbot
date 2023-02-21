using System.Threading.Tasks;
using GrillBot.Common.Managers.Counters;
using GrillBot.Common.Models.Pagination;
using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class InviteRepository : RepositoryBase
{
    public InviteRepository(GrillBotContext context, CounterManager counter) : base(context, counter)
    {
    }

    public async Task<Invite?> FindInviteByCodeAsync(ulong guildId, string code)
    {
        using (CreateCounter())
        {
            return await Context.Invites
                .FirstOrDefaultAsync(o => o.GuildId == guildId.ToString() && o.Code == code);
        }
    }

    public async Task<PaginatedResponse<Invite>> GetInviteListAsync(IQueryableModel<Invite> model, PaginatedParams pagination)
    {
        using (CreateCounter())
        {
            var query = CreateQuery(model, true);
            return await PaginatedResponse<Invite>.CreateWithEntityAsync(query, pagination);
        }
    }
}
