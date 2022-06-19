using System.Linq;
using System.Threading.Tasks;
using Discord;
using GrillBot.Common.Managers.Counters;
using GrillBot.Database.Entity;
using GrillBot.Database.Models;
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

    public async Task<Guild?> FindGuildByIdAsync(ulong id, bool disableTracking = false)
    {
        using (Counter.Create("Database"))
        {
            var query = Context.Guilds.AsQueryable();
            if (disableTracking)
                query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync(o => o.Id == id.ToString());
        }
    }

    public async Task<PaginatedResponse<Guild>> GetGuildListAsync(IQueryableModel<Guild> model, PaginatedParams pagination)
    {
        using (Counter.Create("Database"))
        {
            var query = CreateQuery(model, true);
            return await PaginatedResponse<Guild>.CreateWithEntityAsync(query, pagination);
        }
    }

    public async Task<(int auditLogs, int channels, int invites, int searches, int unverify, int unverifyLogs, int users)> GetDatabaseReportDataAsync(ulong guildId)
    {
        using (Counter.Create("Database"))
        {
            var query = Context.Guilds.AsNoTracking()
                .Where(o => o.Id == guildId.ToString())
                .Select(g => new
                {
                    AuditLogs = g.AuditLogs.Count,
                    Channels = g.Channels.Count,
                    Invites = g.Invites.Count,
                    Searches = g.Searches.Count,
                    Unverifies = g.Unverifies.Count,
                    UnverifyLogs = g.UnverifyLogs.Count,
                    Users = g.Users.Count
                });

            var data = await query.FirstOrDefaultAsync();
            return data == null ? (0, 0, 0, 0, 0, 0, 0) : (data.AuditLogs, data.Channels, data.Invites, data.Searches, data.Unverifies, data.UnverifyLogs, data.Users);
        }
    }

    public async Task<bool> ExistsAsync(IGuild guild)
    {
        using (Counter.Create("Database"))
        {
            return await Context.Guilds.AsNoTracking()
                .AnyAsync(o => o.Id == guild.Id.ToString());
        }
    }
}
