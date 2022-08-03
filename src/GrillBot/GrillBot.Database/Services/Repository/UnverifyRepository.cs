using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using GrillBot.Common.Extensions;
using GrillBot.Common.Managers.Counters;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class UnverifyRepository : RepositoryBase
{
    public UnverifyRepository(GrillBotContext context, CounterManager counter) : base(context, counter)
    {
    }

    public async Task<List<ulong>> GetUserIdsWithUnverify(IGuild guild)
    {
        using (CreateCounter())
        {
            var ids = await Context.Unverifies.AsNoTracking()
                .Where(o => o.GuildId == guild.Id.ToString())
                .Select(o => o.UserId)
                .ToListAsync();

            return ids.ConvertAll(o => o.ToUlong());
        }
    }

    public async Task<(int unverify, int selfunverify)> GetUserStatsAsync(IGuildUser user)
    {
        using (CreateCounter())
        {
            var baseQuery = Context.UnverifyLogs.AsNoTracking()
                .Where(o => o.ToUserId == user.Id.ToString() && o.GuildId == user.GuildId.ToString());

            var unverify = await baseQuery.CountAsync(o => o.Operation == UnverifyOperation.Unverify);
            var selfunverify = await baseQuery.CountAsync(o => o.Operation == UnverifyOperation.Selfunverify);

            return (unverify, selfunverify);
        }
    }

    public async Task<PaginatedResponse<UnverifyLog>> GetLogsAsync(IQueryableModel<UnverifyLog> model, PaginatedParams pagination)
    {
        using (CreateCounter())
        {
            var query = CreateQuery(model, true);
            return await PaginatedResponse<UnverifyLog>.CreateWithEntityAsync(query, pagination);
        }
    }

    public async Task<List<(ulong guildId, ulong userId)>> GetPendingUnverifyIdsAsync()
    {
        using (CreateCounter())
        {
            var data = await Context.Unverifies.AsNoTracking()
                .Where(o => o.EndAt <= DateTime.Now)
                .Select(o => new { o.GuildId, o.UserId })
                .ToListAsync();

            return data.ConvertAll(o => (o.GuildId.ToUlong(), o.UserId.ToUlong()));
        }
    }

    public async Task<UnverifyLog?> FindUnverifyLogByIdAsync(long id)
    {
        using (CreateCounter())
        {
            return await Context.UnverifyLogs.AsNoTracking()
                .Include(o => o.Guild)
                .Include(o => o.ToUser!.Unverify) // TODO Nullable
                .FirstOrDefaultAsync(o => o.Id == id);
        }
    }

    public async Task<List<Unverify>> GetUnverifiesAsync(ulong? userId = null)
    {
        using (CreateCounter())
        {
            var query = Context.Unverifies
                .Include(o => o.UnverifyLog)
                .AsQueryable();

            if (userId != null)
                query = query.Where(o => o.UserId == userId.Value.ToString());

            return await query.ToListAsync();
        }
    }

    public async Task<Unverify?> FindUnverifyPageAsync(IGuild guild, int page)
    {
        using (CreateCounter())
        {
            return await Context.Unverifies.AsNoTracking()
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
            return await Context.Unverifies.AsNoTracking()
                .CountAsync(o => o.GuildId == guild.Id.ToString());
        }
    }

    public async Task<Unverify?> FindUnverifyAsync(ulong guildId, ulong userId)
    {
        using (CreateCounter())
        {
            return await Context.Unverifies
                .FirstOrDefaultAsync(o => o.GuildId == guildId.ToString() && o.UserId == userId.ToString());
        }
    }

    public async Task<Dictionary<UnverifyOperation, int>> GetStatisticsByTypeAsync()
    {
        using (CreateCounter())
        {
            return await Context.UnverifyLogs.AsNoTracking()
                .GroupBy(o => o.Operation)
                .Select(o => new { Type = o.Key, Count = o.Count() })
                .ToDictionaryAsync(o => o.Type, o => o.Count);
        }
    }

    public async Task<Dictionary<string, int>> GetStatisticsByDateAsync()
    {
        using (CreateCounter())
        {
            return await Context.UnverifyLogs.AsNoTracking()
                .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
                .OrderByDescending(o => o.Key.Year).ThenByDescending(o => o.Key.Month)
                .Select(o => new { Date = $"{o.Key.Year}-{o.Key.Month.ToString().PadLeft(2, '0')}", Count = o.Count() })
                .ToDictionaryAsync(o => o.Date, o => o.Count);
        }
    }
}
