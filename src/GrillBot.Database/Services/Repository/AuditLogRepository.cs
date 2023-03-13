using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using GrillBot.Common.Extensions;
using GrillBot.Common.Managers.Counters;
using GrillBot.Common.Models.Pagination;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class AuditLogRepository : RepositoryBase
{
    public AuditLogRepository(GrillBotContext context, CounterManager counter) : base(context, counter)
    {
    }

    public async Task<List<ulong>> GetDiscordAuditLogIdsAsync(IGuild? guild, IChannel? channel, AuditLogItemType[]? types, DateTime after)
    {
        using (CreateCounter())
        {
            var query = Context.AuditLogs.AsNoTracking()
                .Where(o => o.DiscordAuditLogItemId != null && o.CreatedAt >= after);

            if (guild != null)
                query = query.Where(o => o.GuildId == guild.Id.ToString());

            if (channel != null)
                query = query.Where(o => o.ChannelId == channel.Id.ToString());

            if (types?.Length > 0)
                query = query.Where(o => types.Contains(o.Type));

            var ids = await query
                .Select(o => o.DiscordAuditLogItemId!)
                .ToListAsync();

            return ids
                .SelectMany(o => o.Split(','))
                .Select(o => o.Trim().ToUlong())
                .Distinct()
                .ToList();
        }
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

    public async Task<AuditLogItem?> FindLogItemByIdAsync(long id, bool includeFiles = false)
    {
        using (CreateCounter())
        {
            var query = Context.AuditLogs.AsQueryable();
            if (includeFiles)
                query = query.Include(o => o.Files);

            return await query.FirstOrDefaultAsync(o => o.Id == id);
        }
    }

    public async Task<AuditLogItem?> FindLogItemByDiscordIdAsync(ulong auditLogId, AuditLogItemType type)
    {
        using (CreateCounter())
        {
            return await Context.AuditLogs
                .FirstOrDefaultAsync(o => o.DiscordAuditLogItemId == auditLogId.ToString() && o.Type == type);
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
}
