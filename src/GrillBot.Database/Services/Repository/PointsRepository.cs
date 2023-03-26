using System.Threading.Tasks;
using Discord;
using GrillBot.Core.Database.Repository;
using GrillBot.Core.Managers.Performance;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class PointsRepository : RepositoryBase<GrillBotContext>
{
    public PointsRepository(GrillBotContext context, ICounterManager counter) : base(context, counter)
    {
    }

    public async Task<bool> ExistsAnyTransactionAsync(IGuildUser user)
    {
        using (CreateCounter())
        {
            return await Context.PointsTransactions.AsNoTracking()
                .AnyAsync(o => o.GuildId == user.GuildId.ToString() && o.UserId == user.Id.ToString());
        }
    }
}
