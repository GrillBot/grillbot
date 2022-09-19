using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrillBot.Common.Managers.Counters;
using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class AutoReplyRepository : RepositoryBase
{
    public AutoReplyRepository(GrillBotContext context, CounterManager counter) : base(context, counter)
    {
    }

    public async Task<List<AutoReplyItem>> GetAllAsync()
    {
        using (Counter.Create("Database"))
        {
            return await Context.AutoReplies.AsNoTracking()
                .OrderBy(o => o.Id).ToListAsync();
        }
    }

    public async Task<AutoReplyItem?> FindReplyByIdAsync(long id)
    {
        using (Counter.Create("Database"))
        {
            return await Context.AutoReplies
                .FirstOrDefaultAsync(o => o.Id == id);
        }
    }
}
