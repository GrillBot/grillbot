using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrillBot.Core.Database.Repository;
using GrillBot.Core.Managers.Performance;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class AutoReplyRepository : RepositoryBase<GrillBotContext>
{
    public AutoReplyRepository(GrillBotContext context, ICounterManager counter) : base(context, counter)
    {
    }

    public async Task<List<AutoReplyItem>> GetAllAsync(bool onlyEnabled)
    {
        using (CreateCounter())
        {
            var query = Context.AutoReplies.AsNoTracking()
                .OrderBy(o => o.Id).AsQueryable();

            if (onlyEnabled)
                query = query.Where(o => (o.Flags & (long)AutoReplyFlags.Disabled) == 0);
            return await query.ToListAsync();
        }
    }

    public async Task<AutoReplyItem?> FindReplyByIdAsync(long id)
    {
        using (CreateCounter())
        {
            return await Context.AutoReplies
                .FirstOrDefaultAsync(o => o.Id == id);
        }
    }
}
