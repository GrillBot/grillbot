using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrillBot.Core.Database.Repository;
using GrillBot.Core.Managers.Performance;
using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class EmoteRepository : SubRepositoryBase<GrillBotContext>
{
    public EmoteRepository(GrillBotContext context, ICounterManager counter) : base(context, counter)
    {
    }

    public async Task<List<EmoteStatisticItem>> GetAllStatisticsAsync()
    {
        using (CreateCounter())
        {
            return await Context.Emotes.ToListAsync();
        }
    }
}
