using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using GrillBot.Common.Managers.Counters;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
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
    public IEnumerable<PointsTransactionSummary> GetAllSummaries() => Context.PointsTransactionSummaries.AsEnumerable();

    public async Task<long> ComputePointsOfUserAsync(ulong guildId, ulong userId)
    {
        using (CreateCounter())
        {
            var yearBack = DateTime.Now.AddYears(-1);

            return await Context.PointsTransactionSummaries.AsNoTracking()
                .Where(o => !o.IsMerged && o.Day >= yearBack && o.GuildId == guildId.ToString() && o.UserId == userId.ToString())
                .SumAsync(o => o.MessagePoints + o.ReactionPoints);
        }
    }

    public async Task<List<PointsTransaction>> GetAllTransactionsAsync(bool onlyToday, IGuildUser? guildUser)
    {
        using (CreateCounter())
        {
            var query = Context.PointsTransactions.AsNoTracking()
                .Where(o => o.MergedItemsCount == 0);

            if (onlyToday)
            {
                query = query.Where(o => o.AssingnedAt.Date == DateTime.Now.Date);
            }
            else
            {
                var yearBack = DateTime.Now.AddYears(-1);
                query = query.Where(o => o.AssingnedAt >= yearBack);
            }

            if (guildUser != null)
                query = query.Where(o => o.UserId == guildUser.Id.ToString() && o.GuildId == guildUser.GuildId.ToString());

            return await query.ToListAsync();
        }
    }

    public async Task<List<PointsTransactionSummary>> GetSummariesAsync(DateTime from, DateTime to, List<string> guilds, List<string> users)
    {
        using (CreateCounter())
        {
            var query = Context.PointsTransactionSummaries
                .Where(o => !o.IsMerged && o.Day >= from && o.Day <= to && guilds.Contains(o.GuildId) && users.Contains(o.UserId));
            return await query.ToListAsync();
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

            var query = Context.PointsTransactionSummaries.AsNoTracking()
                .Where(o => !o.IsMerged && o.GuildId == user.GuildId.ToString() && o.Day >= yearBack)
                .GroupBy(o => o.UserId)
                .Where(o => o.Sum(x => x.MessagePoints + x.ReactionPoints) > userPoints);

            var count = await query.CountAsync();
            return count + 1;
        }
    }

    public async Task<List<PointBoardItem>> GetPointsBoardDataAsync(IEnumerable<string> guildIds, int? take = null)
    {
        var guildIdData = guildIds.ToList();

        using (CreateCounter())
        {
            if (guildIdData.Count == 0)
                return new List<PointBoardItem>();

            var baseQuery = Context.PointsTransactionSummaries.AsNoTracking()
                .Where(o =>
                    (o.GuildUser.User!.Flags & (int)UserFlags.NotUser) == 0 &&
                    guildIdData.Contains(o.GuildId)
                );

            var query = baseQuery
                .GroupBy(o => new { o.GuildId, o.UserId })
                .Select(o => new PointBoardItem
                {
                    PointsToday = o.Where(x => x.Day == DateTime.Now.Date).Sum(x => x.MessagePoints + x.ReactionPoints),
                    TotalPoints = o.Sum(x => x.MessagePoints + x.ReactionPoints),
                    PointsMonthBack = o.Where(x => x.Day >= DateTime.Now.AddMonths(-1).Date).Sum(x => x.MessagePoints + x.ReactionPoints),
                    PointsYearBack = o.Where(x => x.Day >= DateTime.Now.AddYears(-1).Date).Sum(x => x.MessagePoints + x.ReactionPoints),
                    GuildId = o.Key.GuildId,
                    UserId = o.Key.UserId
                })
                .Where(o => o.TotalPoints > 0)
                .OrderByDescending(o => o.PointsYearBack)
                .AsQueryable();

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

    public async Task<PaginatedResponse<PointsTransactionSummary>> GetSummaryListAsync(IQueryableModel<PointsTransactionSummary> model, PaginatedParams parameters)
    {
        using (CreateCounter())
        {
            var query = CreateQuery(model, true);
            return await PaginatedResponse<PointsTransactionSummary>.CreateWithEntityAsync(query, parameters);
        }
    }

    public async Task<List<PointsTransactionSummary>> GetGraphDataAsync(IQueryableModel<PointsTransactionSummary> model)
    {
        using (CreateCounter())
        {
            var query = CreateQuery(model, true);

            var groupedQuery = query.GroupBy(o => o.Day).Select(o => new PointsTransactionSummary
            {
                Day = o.Key,
                MessagePoints = o.Sum(x => x.MessagePoints),
                ReactionPoints = o.Sum(x => x.ReactionPoints)
            }).OrderBy(o => o.Day);

            return await groupedQuery.ToListAsync();
        }
    }

    private IQueryable<PointsTransaction> GetExpiredTransactionsBaseQuery()
    {
        var expirationDate = DateTime.Now.AddYears(-1).AddMonths(-6);
        return Context.PointsTransactions
            .Include(o => o.GuildUser.User)
            .Include(o => o.Guild)
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

    private IQueryable<PointsTransactionSummary> GetExpiredSummariesBaseQuery()
    {
        var expirationDate = DateTime.Now.AddYears(-1).AddMonths(-6).Date;
        return Context.PointsTransactionSummaries
            .Include(o => o.Guild)
            .Include(o => o.GuildUser.User)
            .Where(o => o.Day <= expirationDate && !o.IsMerged);
    }

    public async Task<bool> ExistsExpiredSummariesAsync()
    {
        using (CreateCounter())
        {
            return await GetExpiredSummariesBaseQuery().AnyAsync();
        }
    }

    public async Task<List<PointsTransactionSummary>> GetExpiredSummariesAsync()
    {
        using (CreateCounter())
        {
            return await GetExpiredSummariesBaseQuery().ToListAsync();
        }
    }
}
