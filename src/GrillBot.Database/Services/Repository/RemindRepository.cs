using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrillBot.Core.Database.Repository;
using GrillBot.Core.Managers.Performance;
using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class RemindRepository : SubRepositoryBase<GrillBotContext>
{
    public RemindRepository(GrillBotContext context, ICounterManager counter) : base(context, counter)
    {
    }

    public async Task<List<RemindMessage>> GetAllRemindersAsync()
    {
        using (CreateCounter())
        {
            return await Context.Reminders.AsNoTracking().ToListAsync();
        }
    }
}
