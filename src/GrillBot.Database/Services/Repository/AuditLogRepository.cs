using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrillBot.Core.Database;
using GrillBot.Core.Database.Repository;
using GrillBot.Core.Managers.Performance;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class AuditLogRepository : RepositoryBase<GrillBotContext>
{
    public AuditLogRepository(GrillBotContext context, ICounterManager counter) : base(context, counter)
    {
    }

    public async Task<List<AuditLogItem>> GetSimpleDataAsync(IQueryableModel<AuditLogItem> model, int? count = null)
    {
        using (CreateCounter())
        {
            var query = CreateQuery(model, true)
                .Select(o => new AuditLogItem { Id = o.Id, Type = o.Type, Data = o.Data, CreatedAt = o.CreatedAt });
            if (count != null)
                query = query.Take(count.Value);

            return await query.ToListAsync();
        }
    }

    public async Task<List<string>> GetOnlyDataAsync(IQueryableModel<AuditLogItem> model, int? count = null)
    {
        using (CreateCounter())
        {
            var query = CreateQuery(model, true)
                .Where(o => !string.IsNullOrEmpty(o.Data))
                .Select(o => o.Data);
            if (count != null)
                query = query.Take(count.Value);
            return await query.ToListAsync();
        }
    }
}
