using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrillBot.Common.Managers.Counters;
using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class ApiClientRepository : RepositoryBase
{
    public ApiClientRepository(GrillBotContext context, CounterManager counter) : base(context, counter)
    {
    }

    public async Task<ApiClient?> FindClientById(string id)
    {
        using (CreateCounter())
        {
            return await Context.ApiClients
                .FirstOrDefaultAsync(o => o.Id == id);
        }
    }

    public async Task<List<ApiClient>> GetClientsAsync()
    {
        using (CreateCounter())
        {
            return await Context.ApiClients.ToListAsync();
        }
    }
}
