using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrillBot.Common.Managers.Counters;
using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class RemindRepository : RepositoryBase
{
    public RemindRepository(GrillBotContext context, CounterManager counter) : base(context, counter)
    {
    }

    public async Task<List<long>> GetRemindIdsForProcessAsync()
    {
        using (Counter.Create("Database"))
        {
            return await Context.Reminders.AsNoTracking()
                .Where(o => o.RemindMessageId == null && o.At <= DateTime.Now)
                .Select(o => o.Id)
                .ToListAsync();
        }
    }

    public async Task<RemindMessage?> FindRemindByIdAsync(long id)
    {
        using (Counter.Create("Database"))
        {
            return await Context.Reminders
                .FirstOrDefaultAsync(o => o.Id == id);
        }
    }
}
