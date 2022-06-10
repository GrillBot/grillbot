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
            var entity = await Context.Guilds
                .FirstOrDefaultAsync(o => o.Id == guild.Id.ToString());

            if (entity != null)
                return entity;

            entity = Guild.FromDiscord(guild);
            await Context.AddAsync(entity);
            return entity;
        }
    }
}
