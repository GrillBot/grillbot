using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrillBot.Core.Database.Repository;
using GrillBot.Core.Managers.Performance;
using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class SelfUnverifyRepository : SubRepositoryBase<GrillBotContext>
{
    public SelfUnverifyRepository(GrillBotContext context, ICounterManager counter) : base(context, counter)
    {
    }

    public async Task<List<SelfunverifyKeepable>> GetKeepablesAsync(string? group = null, bool exactMatch = false)
    {
        using (CreateCounter())
        {
            var query = DbContext.SelfunverifyKeepables
                .OrderBy(o => o.GroupName).ThenBy(o => o.Name)
                .AsQueryable();

            if (!string.IsNullOrEmpty(group))
                query = exactMatch ? query.Where(o => o.GroupName == group.ToLower()) : query.Where(o => o.GroupName.StartsWith(group.ToLower()));

            return await query.ToListAsync();
        }
    }
}
