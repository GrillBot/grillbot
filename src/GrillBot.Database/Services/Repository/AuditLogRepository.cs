using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GrillBot.Core.Database;
using GrillBot.Core.Database.Repository;
using GrillBot.Core.Managers.Performance;
using GrillBot.Core.Models.Pagination;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class AuditLogRepository : RepositoryBase<GrillBotContext>
{
    public AuditLogRepository(GrillBotContext context, ICounterManager counter) : base(context, counter)
    {
    }

    public async Task<bool> ExistsExpiredItemAsync(DateTime expiredAt)
    {
        using (CreateCounter())
        {
            return await GetExpiredItemsQueryAsync(expiredAt).AnyAsync();
        }
    }

    public async Task<List<AuditLogItem>> GetExpiredDataAsync(DateTime expiredAt)
    {
        using (CreateCounter())
        {
            return await GetExpiredItemsQueryAsync(expiredAt).ToListAsync();
        }
    }

    private IQueryable<AuditLogItem> GetExpiredItemsQueryAsync(DateTime expiredAt)
    {
        return Context.AuditLogs.AsQueryable()
            .Include(o => o.Files)
            .Include(o => o.Guild)
            .Include(o => o.GuildChannel)
            .Include(o => o.ProcessedGuildUser!.User)
            .Include(o => o.ProcessedUser)
            .Where(o => o.CreatedAt <= expiredAt);
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

    public async Task<PaginatedResponse<AuditLogItem>> GetLogListAsync(IQueryableModel<AuditLogItem> model, PaginatedParams pagination, List<long>? logIds)
    {
        using (CreateCounter())
        {
            var query = CreateQuery(model, true);
            if (logIds != null)
                query = query.Where(o => logIds.Contains(o.Id));

            return await PaginatedResponse<AuditLogItem>.CreateWithEntityAsync(query, pagination);
        }
    }

    public async Task<Dictionary<AuditLogItemType, int>> GetStatisticsByTypeAsync()
    {
        using (CreateCounter())
        {
            return await Context.AuditLogs.AsNoTracking()
                .GroupBy(o => o.Type)
                .Select(o => new { Type = o.Key, Count = o.Count() })
                .ToDictionaryAsync(o => o.Type, o => o.Count);
        }
    }

    public async Task<Dictionary<string, int>> GetStatisticsByDateAsync()
    {
        using (CreateCounter())
        {
            return await Context.AuditLogs.AsNoTracking()
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .OrderBy(o => o.Key.Year).ThenBy(o => o.Key.Month)
                .Select(o => new { Date = $"{o.Key.Year}-{o.Key.Month.ToString().PadLeft(2, '0')}", Count = o.Count() })
                .ToDictionaryAsync(o => o.Date, o => o.Count);
        }
    }

    public async Task<List<AuditLogFileMeta>> GetAllFilesAsync()
    {
        using (CreateCounter())
        {
            return await Context.AuditLogFiles.AsNoTracking()
                .OrderBy(o => o.Id)
                .ToListAsync();
        }
    }

    public async Task<List<AuditLogItem>> GetItemsByType(AuditLogItemType type)
        => await Context.AuditLogs.Include(o => o.Files).Where(o => o.Type == type).OrderByDescending(o => o.Id).ToListAsync();
}
