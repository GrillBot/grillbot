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
        using (CreateCounter())
        {
            return await Context.Reminders.AsNoTracking()
                .Where(o => o.RemindMessageId == null && o.At <= DateTime.Now)
                .Select(o => o.Id)
                .ToListAsync();
        }
    }

    public async Task<RemindMessage?> FindRemindByIdAsync(long id)
    {
        using (CreateCounter())
        {
            return await Context.Reminders
                .FirstOrDefaultAsync(o => o.Id == id);
        }
    }

    public async Task<RemindMessage?> FindRemindByRemindMessageAsync(string messageId)
    {
        using (CreateCounter())
        {
            return await Context.Reminders
                .FirstOrDefaultAsync(o => o.RemindMessageId == messageId);
        }
    }

    public async Task<int> GetRemindersCountAsync(IQueryableModel<RemindMessage> model)
    {
        using (CreateCounter())
        {
            return await CreateQuery(model, true).CountAsync();
        }
    }

    public async Task<PaginatedResponse<RemindMessage>> GetRemindListAsync(IQueryableModel<RemindMessage> model, PaginatedParams pagination)
    {
        using (CreateCounter())
        {
            var query = CreateQuery(model, true);
            return await PaginatedResponse<RemindMessage>.CreateWithEntityAsync(query, pagination);
        }
    }

    public async Task<bool> ExistsCopyAsync(string? originalMessageId, IUser toUser)
    {
        using (CreateCounter())
        {
            return await Context.Reminders.AsNoTracking()
                .AnyAsync(o => o.OriginalMessageId == originalMessageId && o.ToUserId == toUser.Id.ToString());
        }
    }

    public async Task<List<RemindMessage>> GetRemindSuggestionsAsync(IUser user)
    {
        using (CreateCounter())
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
