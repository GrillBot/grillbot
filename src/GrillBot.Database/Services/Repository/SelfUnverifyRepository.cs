using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrillBot.Core.Database.Repository;
using GrillBot.Core.Managers.Performance;
using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class SelfUnverifyRepository : RepositoryBase<GrillBotContext>
{
    public SelfUnverifyRepository(GrillBotContext context, ICounterManager counter) : base(context, counter)
    {
    }

    public async Task<bool> KeepableExistsAsync(string group, string? name = null)
    {
        using (CreateCounter())
        {
            var query = Context.SelfunverifyKeepables.AsNoTracking()
                .Where(o => o.GroupName == group.ToLower());

            if (!string.IsNullOrEmpty(name))
                query = query.Where(o => o.Name == name.ToLower());

            return await query.AnyAsync();
        }
    }

    public async Task<List<SelfunverifyKeepable>> GetKeepablesAsync(string? group = null, bool exactMatch = false)
    {
        using (CreateCounter())
        {
            var query = Context.SelfunverifyKeepables
                .OrderBy(o => o.GroupName).ThenBy(o => o.Name)
                .AsQueryable();

            if (!string.IsNullOrEmpty(group))
                query = exactMatch ? query.Where(o => o.GroupName == group.ToLower()) : query.Where(o => o.GroupName.StartsWith(group.ToLower()));

            return await query.ToListAsync();
        }
    }

    public async Task<SelfunverifyKeepable?> FindKeepableAsync(string group, string name)
    {
        using (CreateCounter())
        {
            return await Context.SelfunverifyKeepables
                .FirstOrDefaultAsync(o => o.GroupName == group.ToLower() && o.Name == name.ToLower());
        }
    }
}
