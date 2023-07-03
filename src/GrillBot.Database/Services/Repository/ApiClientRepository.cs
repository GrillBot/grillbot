using System.Collections.Generic;
using System.Threading.Tasks;
using GrillBot.Core.Database.Repository;
using GrillBot.Core.Managers.Performance;
using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class ApiClientRepository : SubRepositoryBase<GrillBotContext>
{
    public ApiClientRepository(GrillBotContext context, ICounterManager counter) : base(context, counter)
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
