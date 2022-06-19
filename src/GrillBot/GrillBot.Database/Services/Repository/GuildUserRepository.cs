using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using GrillBot.Common.Managers.Counters;
using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using Microsoft.EntityFrameworkCore;

namespace GrillBot.Database.Services.Repository;

public class GuildUserRepository : RepositoryBase
{
    public GuildUserRepository(GrillBotContext context, CounterManager counter) : base(context, counter)
    {
    }

    public async Task<GuildUser> GetOrCreateGuildUserAsync(IGuildUser user)
    {
        using (Counter.Create("Database"))
        {
            var entity = await FindGuildUserAsync(user);
            if (entity != null)
                return entity;

            entity = GuildUser.FromDiscord(user.Guild, user);
            await Context.AddAsync(entity);

            return entity;
        }
    }

    public async Task<GuildUser?> FindGuildUserAsync(IGuildUser user, bool disableTracking = false)
    {
        using (Counter.Create("Database"))
        {
            var query = Context.GuildUsers
                .Include(o => o.Guild)
                .Include(o => o.User)
                .Include(o => o.UsedInvite!.Creator) // TODO Null operator in lambda queries.
                .Include(o => o.Unverify!.UnverifyLog)
                .AsQueryable();

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

    public async Task<int> CalculatePointsPositionAsync(IGuildUser user)
    {
        using (Counter.Create("Database"))
        {
            var query = Context.GuildUsers.AsNoTracking()
                .Where(o =>
                    o.GuildId == user.GuildId.ToString() &&
                    o.UserId == user.Id.ToString() &&
                    (o.User!.Flags & (int)UserFlags.NotUser) == 0 &&
                    !o.User!.Username.StartsWith("Imported")
                )
                .Select(o => o.Points)
                .SelectMany(pts =>
                    Context.GuildUsers.AsNoTracking()
                        .Where(o =>
                            o.GuildId == user.GuildId.ToString() &&
                            o.Points > pts &&
                            (o.User!.Flags & (int)UserFlags.NotUser) == 0 &&
                            !o.User.Username.StartsWith("Imported")
                        )
                );

            var count = await query.CountAsync();
            return count + 1;
        }
    }

    public async Task<List<GuildUser>> GetPointsBoardDataAsync(IEnumerable<string> guildIds, int? take = null)
    {
        using (Counter.Create("Database"))
        {
            var query = Context.GuildUsers.AsNoTracking()
                .Include(o => o.Guild)
                .Include(o => o.User)
                .Where(o =>
                    o.Points > 0 &&
                    (o.User!.Flags & (int)UserFlags.NotUser) == 0 &&
                    guildIds.Contains(o.GuildId) &&
                    !o.User.Username.StartsWith("Imported")
                )
                .OrderByDescending(o => o.Points)
                .ThenBy(o => o.Nickname)
                .ThenBy(o => o.User!.Username)
                .ThenBy(o => o.User!.Discriminator)
                .AsQueryable();

            if (take != null)
                query = query.Take(take.Value);

            return await query.ToListAsync();
        }
    }

    public async Task<bool> ExistsAsync(IGuildUser user)
    {
        using (Counter.Create("Database"))
        {
            return await Context.GuildUsers.AsNoTracking()
                .AnyAsync(o => o.UserId == user.Id.ToString() && o.GuildId == user.GuildId.ToString());
        }
    }

    public async Task<List<GuildUser>> GetAllUsersAsync()
    {
        using (Counter.Create("Database"))
        {
            return await Context.GuildUsers
                .Include(o => o.User)
                .ToListAsync();
        }
    }
}
