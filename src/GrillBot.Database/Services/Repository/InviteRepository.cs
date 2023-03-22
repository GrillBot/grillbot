using System.Threading.Tasks;
using GrillBot.Core.Database;
using GrillBot.Core.Database.Repository;
using GrillBot.Core.Managers.Performance;
using GrillBot.Core.Models.Pagination;
using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class InviteRepository : RepositoryBase<GrillBotContext>
{
    public InviteRepository(GrillBotContext context, ICounterManager counter) : base(context, counter)
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
