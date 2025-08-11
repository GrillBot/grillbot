using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using GrillBot.Core.Database;
using GrillBot.Core.Database.Repository;
using GrillBot.Core.Managers.Performance;
using GrillBot.Core.Models.Pagination;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class UnverifyRepository : SubRepositoryBase<GrillBotContext>
{
    public UnverifyRepository(GrillBotContext context, ICounterManager counter) : base(context, counter)
    {
    }

    public async Task<(int unverify, int selfunverify)> GetUserStatsAsync(string guildId, string userId)
    {
        using (CreateCounter())
        {
            var data = await DbContext.UnverifyLogs.AsNoTracking()
                .Where(o => o.ToUserId == userId && o.GuildId == guildId && (o.Operation == UnverifyOperation.Unverify || o.Operation == UnverifyOperation.Selfunverify))
                .GroupBy(o => o.Operation)
                .Select(o => new { o.Key, Count = o.Count() })
                .ToDictionaryAsync(o => o.Key, o => o.Count);

            return (
                data.TryGetValue(UnverifyOperation.Unverify, out var unverifyCount) ? unverifyCount : 0,
                data.TryGetValue(UnverifyOperation.Selfunverify, out var selfUnverifyCount) ? selfUnverifyCount : 0
            );
        }
    }

    public async Task<Unverify?> GetFirstPendingUnverifyAsync()
    {
        using (CreateCounter())
        {
            return await DbContext.Unverifies.AsNoTracking()
                .Include(o => o.GuildUser!.User).Include(o => o.Guild)
                .FirstOrDefaultAsync(o => o.EndAt <= DateTime.Now);
        }
    }

    public async Task<Unverify?> FindUnverifyPageAsync(IGuild guild, int page)
    {
        using (CreateCounter())
        {
            return await DbContext.Unverifies.AsNoTracking()
                .Include(o => o.UnverifyLog)
                .Where(o => o.GuildId == guild.Id.ToString())
                .OrderBy(o => o.StartAt)
                .ThenBy(o => o.EndAt)
                .Skip(page)
                .FirstOrDefaultAsync();
        }
    }

    public async Task<int> GetUnverifyCountsAsync(IGuild guild)
    {
        using (CreateCounter())
        {
            return await DbContext.Unverifies.AsNoTracking()
                .CountAsync(o => o.GuildId == guild.Id.ToString());
        }
    }

    public async Task<Unverify?> FindUnverifyAsync(ulong guildId, ulong userId, bool disableTracking = false, bool includeLogs = false)
    {
        using (CreateCounter())
        {
            var query = DbContext.Unverifies.AsQueryable();
            if (includeLogs)
                query = query.Include(o => o.UnverifyLog);
            if (disableTracking)
                query = query.AsNoTracking();

            return await query
                .FirstOrDefaultAsync(o => o.GuildId == guildId.ToString() && o.UserId == userId.ToString());
        }
    }

    public async Task<Dictionary<UnverifyOperation, int>> GetStatisticsByTypeAsync()
    {
        using (CreateCounter())
        {
            return await DbContext.UnverifyLogs.AsNoTracking()
                .GroupBy(o => o.Operation)
                .Select(o => new { Type = o.Key, Count = o.Count() })
                .ToDictionaryAsync(o => o.Type, o => o.Count);
        }
    }

    public async Task<Dictionary<string, int>> GetStatisticsByDateAsync()
    {
        using (CreateCounter())
        {
            return await DbContext.UnverifyLogs.AsNoTracking()
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .OrderBy(o => o.Key.Year).ThenBy(o => o.Key.Month)
                .Select(o => new { Date = $"{o.Key.Year}-{o.Key.Month.ToString().PadLeft(2, '0')}", Count = o.Count() })
                .ToDictionaryAsync(o => o.Date, o => o.Count);
        }
    }

    private IQueryable<UnverifyLog> GetLogsForArchivationQuery(DateTime expirationMilestone)
    {
        return DbContext.UnverifyLogs.AsQueryable()
            .Include(o => o.Guild)
            .Include(o => o.FromUser!.User)
            .Include(o => o.ToUser!.User)
            .Where(o => o.CreatedAt <= expirationMilestone);
    }

    public async Task<int> GetCountForArchivationAsync(DateTime expirationMilestone)
    {
        using (CreateCounter())
            return await GetLogsForArchivationQuery(expirationMilestone).CountAsync();
    }

    public async Task<List<UnverifyLog>> GetLogsForArchivationAsync(DateTime exiprationMilestone)
    {
        using (CreateCounter())
        {
            return await GetLogsForArchivationQuery(exiprationMilestone).ToListAsync();
        }
    }
}
