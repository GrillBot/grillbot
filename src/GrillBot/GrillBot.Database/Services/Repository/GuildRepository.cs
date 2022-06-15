using System.Threading.Tasks;
using Discord;
using GrillBot.Common.Managers.Counters;
using GrillBot.Database.Entity;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class GuildRepository : RepositoryBase
{
    public GuildRepository(GrillBotContext context, CounterManager counter) : base(context, counter)
    {
    }

    public async Task<Guild> GetOrCreateRepositoryAsync(IGuild guild)
    {
        using (Counter.Create("Database"))
        {
            var entity = await FindGuildAsync(guild);
            if (entity != null)
                return entity;

            entity = Guild.FromDiscord(guild);
            if (!Context.IsEntityTracked<Guild>(entry => entry.Entity.Id == entity.Id))
                await Context.AddAsync(entity);

            return entity;
        }
    }

    public async Task<Guild?> FindGuildAsync(IGuild guild, bool disableTracking = false)
    {
        using (Counter.Create("Database"))
        {
            var query = Context.Guilds.AsQueryable();
            if (disableTracking)
                query = query.AsNoTracking();

            var entity = await query.FirstOrDefaultAsync(o => o.Id == guild.Id.ToString());
            if (entity == null)
                return null;

            if (!disableTracking)
                entity.Update(guild);
            return entity;
        }
    }
}
