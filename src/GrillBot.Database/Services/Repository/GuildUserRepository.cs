using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using GrillBot.Core.Database.Repository;
using GrillBot.Core.Managers.Performance;
using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class GuildUserRepository : SubRepositoryBase<GrillBotContext>
{
    public GuildUserRepository(GrillBotContext context, ICounterManager counter) : base(context, counter)
    {
    }

    public async Task<GuildUser> GetOrCreateGuildUserAsync(IGuildUser user)
    {
        using (CreateCounter())
        {
            var entity = await FindGuildUserAsync(user, false);
            if (entity != null)
                return entity;

            entity = GuildUser.FromDiscord(user.Guild, user);
            await DbContext.AddAsync(entity);

            return entity;
        }
    }

    public async Task<GuildUser?> FindGuildUserAsync(IGuildUser user, bool disableTracking = false)
    {
        var guildUser = await FindGuildUserByIdAsync(user.GuildId, user.Id, disableTracking);
        if (guildUser is null) return null;

        if (!disableTracking)
            guildUser.Update(user);
        return guildUser;
    }

    public async Task<GuildUser?> FindGuildUserByIdAsync(ulong guildId, ulong userId, bool disableTracking = false)
    {
        using (CreateCounter())
        {
            var query = DbContext.GuildUsers
                .Include(o => o.Guild).Include(o => o.User)
                .AsQueryable();

            if (disableTracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(o => o.UserId == userId.ToString() && o.GuildId == guildId.ToString());
        }
    }

    public async Task<bool> ExistsAsync(IGuildUser user)
    {
        using (CreateCounter())
        {
            return await DbContext.GuildUsers.AsNoTracking()
                .AnyAsync(o => o.UserId == user.Id.ToString() && o.GuildId == user.GuildId.ToString());
        }
    }

    public async Task<List<GuildUser>> GetAllUsersAsync()
    {
        using (CreateCounter())
        {
            return await DbContext.GuildUsers
                .Include(o => o.User)
                .ToListAsync();
        }
    }
}
