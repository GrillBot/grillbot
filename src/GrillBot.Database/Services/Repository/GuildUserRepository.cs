using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using GrillBot.Common.Managers.Counters;
using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class GuildUserRepository : RepositoryBase
{
    public GuildUserRepository(GrillBotContext context, CounterManager counter) : base(context, counter)
    {
    }

    public async Task<GuildUser> GetOrCreateGuildUserAsync(IGuildUser user, bool includeAll = false)
    {
        using (CreateCounter())
        {
            var entity = await FindGuildUserAsync(user, false, includeAll);
            if (entity != null)
                return entity;

            entity = GuildUser.FromDiscord(user.Guild, user);
            await Context.AddAsync(entity);

            return entity;
        }
    }

    public async Task<GuildUser?> FindGuildUserAsync(IGuildUser user, bool disableTracking = false, bool includeAll = false)
    {
        using (CreateCounter())
        {
            var query = Context.GuildUsers
                .Include(o => o.Guild).Include(o => o.User)
                .AsQueryable();

            if (includeAll)
            {
                query = query
                    .Include(o => o.UsedInvite!.Creator!.User)
                    .Include(o => o.Unverify!.UnverifyLog);
            }

            if (disableTracking)
                query = query.AsNoTracking();

            var entity = await query.FirstOrDefaultAsync(o => o.UserId == user.Id.ToString() && o.GuildId == user.GuildId.ToString());
            if (entity == null)
                return null;

            if (!disableTracking)
                entity.Update(user);
            return entity;
        }
    }

    public async Task<bool> ExistsAsync(IGuildUser user)
    {
        using (CreateCounter())
        {
            return await Context.GuildUsers.AsNoTracking()
                .AnyAsync(o => o.UserId == user.Id.ToString() && o.GuildId == user.GuildId.ToString());
        }
    }

    public async Task<List<GuildUser>> GetAllUsersAsync()
    {
        using (CreateCounter())
        {
            return await Context.GuildUsers
                .Include(o => o.User)
                .Include(o => o.Nicknames)
                .ToListAsync();
        }
    }

    public async Task<List<GuildUser>> FindUsersWithInviteCode(ulong guildId, string code)
    {
        using (CreateCounter())
        {
            return await Context.GuildUsers
                .Where(o => o.GuildId == guildId.ToString() && o.UsedInviteCode == code)
                .ToListAsync();
        }
    }
}
