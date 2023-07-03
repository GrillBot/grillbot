using System.Linq;
using System.Threading.Tasks;
using Discord;
using GrillBot.Core.Database;
using GrillBot.Core.Database.Repository;
using GrillBot.Core.Managers.Performance;
using GrillBot.Core.Models.Pagination;
using GrillBot.Database.Entity;
using GrillBot.Database.Models.Guilds;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class GuildRepository : SubRepositoryBase<GrillBotContext>
{
    public GuildRepository(GrillBotContext context, ICounterManager counter) : base(context, counter)
    {
    }

    public async Task<Guild> GetOrCreateGuildAsync(IGuild guild)
    {
        using (CreateCounter())
        {
            var entity = await FindGuildAsync(guild);
            if (entity != null)
                return entity;

            entity = Guild.FromDiscord(guild);
            await Context.AddAsync(entity);

            return entity;
        }
    }

    public async Task<Guild?> FindGuildAsync(IGuild guild, bool disableTracking = false)
    {
        using (CreateCounter())
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

    public async Task<Guild?> FindGuildByIdAsync(ulong id, bool disableTracking = false)
    {
        using (CreateCounter())
        {
            var query = Context.Guilds.AsQueryable();
            if (disableTracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(o => o.Id == id.ToString());
        }
    }

    public async Task<PaginatedResponse<Guild>> GetGuildListAsync(IQueryableModel<Guild> model, PaginatedParams pagination)
    {
        using (CreateCounter())
        {
            var query = CreateQuery(model, true);
            return await PaginatedResponse<Guild>.CreateWithEntityAsync(query, pagination);
        }
    }

    public async Task<GuildDatabaseReport> GetDatabaseReportDataAsync(ulong guildId)
    {
        using (CreateCounter())
        {
            var query = Context.Guilds.AsNoTracking()
                .Where(o => o.Id == guildId.ToString())
                .Select(g => new GuildDatabaseReport
                {
                    Channels = g.Channels.Count,
                    Invites = g.Invites.Count,
                    Searches = g.Searches.Count,
                    Unverifies = g.Unverifies.Count,
                    UnverifyLogs = g.UnverifyLogs.Count,
                    Users = g.Users.Count,
                    EmoteStats = g.EmoteStatistics.Count,
                    EmoteSuggestions = Context.EmoteSuggestions.Count(o => o.GuildId == g.Id)
                });

            var data = await query.FirstOrDefaultAsync();
            return data ?? new GuildDatabaseReport();
        }
    }
}
