using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using GrillBot.Common.Managers.Counters;
using GrillBot.Database.Entity;
using GrillBot.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class RemindRepository : RepositoryBase
{
    public RemindRepository(GrillBotContext context, CounterManager counter) : base(context, counter)
    {
    }

    public async Task<List<long>> GetRemindIdsForProcessAsync()
    {
        using (Counter.Create("Database"))
        {
            return await Context.Reminders.AsNoTracking()
                .Where(o => o.RemindMessageId == null && o.At <= DateTime.Now)
                .Select(o => o.Id)
                .ToListAsync();
        }
    }

    public async Task<RemindMessage?> FindRemindByIdAsync(long id)
    {
        using (Counter.Create("Database"))
        {
            return await Context.Reminders
                .FirstOrDefaultAsync(o => o.Id == id);
        }
    }

    public async Task<RemindMessage?> FindRemindByRemindMessageAsync(string messageId)
    {
        using (Counter.Create("Database"))
        {
            return await Context.Reminders
                .FirstOrDefaultAsync(o => o.RemindMessageId == messageId && o.At < DateTime.Now);
        }
    }

    public async Task<int> GetRemindersCountAsync(IUser forUser)
    {
        using (Counter.Create("Database"))
        {
            return await Context.Reminders.AsNoTracking()
                .CountAsync(o => o.ToUserId == forUser.Id.ToString() && o.RemindMessageId == null);
        }
    }

    public async Task<List<RemindMessage>> GetRemindersPageAsync(IUser forUser, int page)
    {
        using (Counter.Create("Database"))
        {
            return await Context.Reminders.AsNoTracking()
                .Where(o => o.ToUserId == forUser.Id.ToString() && o.RemindMessageId == null)
                .OrderBy(o => o.At).ThenBy(o => o.Id)
                .Skip(page * EmbedBuilder.MaxFieldCount)
                .Take(EmbedBuilder.MaxFieldCount)
                .ToListAsync();
        }
    }

    public async Task<PaginatedResponse<RemindMessage>> GetRemindListAsync(IQueryableModel<RemindMessage> model,
        PaginatedParams pagination)
    {
        using (Counter.Create("Database"))
        {
            var query = CreateQuery(model, true);
            return await PaginatedResponse<RemindMessage>.CreateWithEntityAsync(query, pagination);
        }
    }

    public async Task<bool> ExistsCopyAsync(string? originalMessageId, IUser toUser)
    {
        using (Counter.Create("Database"))
        {
            return await Context.Reminders.AsNoTracking()
                .AnyAsync(o => o.OriginalMessageId == originalMessageId && o.ToUserId == toUser.Id.ToString());
        }
    }

    public async Task<List<RemindMessage>> GetRemindSuggestionsAsync(IUser user)
    {
        using (Counter.Create("Database"))
        {
            var userId = user.Id.ToString();

            return await Context.Reminders.AsNoTracking()
                .Include(o => o.FromUser)
                .Include(o => o.ToUser)
                .Where(o => (o.FromUserId == userId || o.ToUserId == userId) && o.RemindMessageId == null)
                .ToListAsync();
        }
    }
}
