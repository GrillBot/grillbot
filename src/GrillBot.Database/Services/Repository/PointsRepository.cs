using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using GrillBot.Common.Managers.Counters;
using GrillBot.Database.Entity;
using GrillBot.Database.Models;
using GrillBot.Database.Models.Points;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class PointsRepository : RepositoryBase
{
    public PointsRepository(GrillBotContext context, CounterManager counter) : base(context, counter)
    {
    }

    // Only for testing purposes.
    public IEnumerable<PointsTransaction> GetAll() => Context.PointsTransactions.AsEnumerable();

    public async Task<long> ComputePointsOfUserAsync(ulong guildId, ulong userId)
    {
        using (CreateCounter())
        {
            var yearBack = DateTime.Now.AddYears(-1);

            return await Context.PointsTransactions.AsNoTracking()
                .Where(o => o.MergedItemsCount == 0 && o.AssingnedAt >= yearBack && o.GuildId == guildId.ToString() && o.UserId == userId.ToString())
                .SumAsync(o => o.Points);
        }
    }

    public async Task<PointsTransaction?> FindTransactionAsync(IGuild? guild, ulong messageId, string? reactionId, IUser? user)
    {
        using (CreateCounter())
        {
            var query = Context.PointsTransactions
                .Where(o => o.MergedItemsCount == 0 && o.MessageId == messageId.ToString());

            if (!string.IsNullOrEmpty(reactionId))
                query = query.Where(o => o.ReactionId == reactionId);
            else
                query = query.Where(o => o.ReactionId == "");

            if (guild != null)
                query = query.Where(o => o.GuildId == guild.Id.ToString());
            if (user != null)
                query = query.Where(o => o.UserId == user.Id.ToString());

            return await query.FirstOrDefaultAsync();
        }
    }

    public async Task<List<PointsTransaction>> GetTransactionsAsync(ulong messageId, IGuild? guild, IUser? user)
    {
        using (CreateCounter())
        {
            var query = Context.PointsTransactions
                .Where(o => o.MergedItemsCount == 0 && o.MessageId == messageId.ToString());

            if (guild != null)
                query = query.Where(o => o.GuildId == guild.Id.ToString());
            if (user != null)
                query = query.Where(o => o.UserId == user.Id.ToString());

            return await query.ToListAsync();
        }
    }

    public async Task<int> CalculatePointsPositionAsync(IGuildUser user, long userPoints)
    {
        using (CreateCounter())
        {
            var yearBack = DateTime.Now.AddYears(-1);

            var query = Context.PointsTransactions.AsNoTracking()
                .Where(o => o.MergedItemsCount == 0 && o.GuildId == user.GuildId.ToString() && o.AssingnedAt >= yearBack)
                .GroupBy(o => o.UserId)
                .Where(o => o.Sum(x => x.Points) > userPoints);

            var count = await query.CountAsync();
            return count + 1;
        }
    }

    private IQueryable<PointBoardItem> GetPointsBoardQuery(IEnumerable<string> guildIds, ulong userId = 0, bool allColumns = false)
    {
        var baseQuery = Context.PointsTransactions.AsNoTracking()
            .Where(o => guildIds.Contains(o.GuildId));

        if (userId > 0)
            baseQuery = baseQuery.Where(o => o.UserId == userId.ToString());

        return baseQuery
            .GroupBy(o => new { o.GuildId, o.UserId })
            .Select(o => new PointBoardItem
            {
                PointsToday = allColumns ? o.Where(x => x.AssingnedAt.Date == DateTime.Now.Date).Sum(x => x.Points) : 0,
                TotalPoints = allColumns ? o.Sum(x => x.Points) : 0,
                PointsMonthBack = allColumns ? o.Where(x => x.AssingnedAt.Date >= DateTime.Now.AddMonths(-1).Date).Sum(x => x.Points) : 0,
                PointsYearBack = o.Where(x => x.AssingnedAt.Date >= DateTime.Now.AddYears(-1).Date).Sum(x => x.Points),
                GuildId = o.Key.GuildId,
                UserId = o.Key.UserId
            })
            .OrderByDescending(o => o.PointsYearBack)
            .AsQueryable();
    }

    public async Task<int> GetPointsBoardCountAsync(IEnumerable<string> guildIds, ulong userId = 0)
    {
        var guildIdData = guildIds.ToList();

        using (CreateCounter())
        {
            if (guildIdData.Count == 0) return 0;
            return await GetPointsBoardQuery(guildIdData, userId).CountAsync();
        }
    }

    public async Task<List<PointBoardItem>> GetPointsBoardDataAsync(IEnumerable<string> guildIds, int? take = null, ulong userId = 0, int? skip = null, bool allColumns = false)
    {
        var guildIdData = guildIds.ToList();

        using (CreateCounter())
        {
            if (guildIdData.Count == 0)
                return new List<PointBoardItem>();

            var query = GetPointsBoardQuery(guildIdData, userId, allColumns);
            if (skip != null)
                query = query.Skip(skip.Value);
            if (take != null)
                query = query.Take(take.Value);

            var data = await query.ToListAsync();
            if (data.Count == 0)
                return data;

            var uniqueUsers = data.GroupBy(o => new { o.GuildId, o.UserId });
            foreach (var userGroup in uniqueUsers)
            {
                var guildUser = await Context.GuildUsers.AsNoTracking()
                    .Include(o => o.Guild)
                    .Include(o => o.User)
                    .FirstOrDefaultAsync(o => o.GuildId == userGroup.Key.GuildId && o.UserId == userGroup.Key.UserId);

                foreach (var user in userGroup)
                    user.GuildUser = guildUser!;
            }

            return data
                .OrderByDescending(o => o.PointsYearBack)
                .ThenBy(o => o.GuildUser.FullName())
                .ToList();
        }
    }

    public async Task<PaginatedResponse<PointsTransaction>> GetTransactionListAsync(IQueryableModel<PointsTransaction> model, PaginatedParams pagination)
    {
        using (CreateCounter())
        {
            var query = CreateQuery(model, true);
            return await PaginatedResponse<PointsTransaction>.CreateWithEntityAsync(query, pagination);
        }
    }

    public async Task<List<(DateTime day, int messagePoints, int reactionPoints)>> GetGraphDataAsync(IQueryableModel<PointsTransaction> model)
    {
        using (CreateCounter())
        {
            var query = CreateQuery(model, true);
            var group = query.GroupBy(o => o.AssingnedAt.Date);
            var groupedQuery = group.Select(o => new
            {
                Date = o.Key,
                MessagePoints = o.Where(x => string.IsNullOrEmpty(x.ReactionId)).Sum(x => x.Points),
                ReactionPoints = o.Where(x => !string.IsNullOrEmpty(x.ReactionId)).Sum(x => x.Points)
            }).OrderBy(o => o.Date);

            var data = await groupedQuery.ToListAsync();
            return data.ConvertAll(o => (o.Date, o.MessagePoints, o.ReactionPoints));
        }
    }

    private IQueryable<PointsTransaction> GetExpiredTransactionsBaseQuery()
    {
        var expirationDate = DateTime.Now.AddYears(-1).AddMonths(-2);
        return Context.PointsTransactions
            .Where(o => o.AssingnedAt <= expirationDate && o.MergedItemsCount == 0); // Select only expired and non merged records.
    }

    public async Task<bool> ExistsExpiredItemsAsync()
    {
        using (CreateCounter())
        {
            return await GetExpiredTransactionsBaseQuery().AnyAsync();
        }
    }

    public async Task<List<PointsTransaction>> GetExpiredTransactionsAsync()
    {
        using (CreateCounter())
        {
            return await GetExpiredTransactionsBaseQuery().ToListAsync();
        }
    }

    public async Task<bool> ExistsTransactionAsync(PointsTransaction transaction)
    {
        using (CreateCounter())
        {
            return await Context.PointsTransactions.AsNoTracking()
                .AnyAsync(o => o.GuildId == transaction.GuildId && o.UserId == transaction.UserId && o.MessageId == transaction.MessageId && o.ReactionId == transaction.ReactionId);
        }
    }
}
