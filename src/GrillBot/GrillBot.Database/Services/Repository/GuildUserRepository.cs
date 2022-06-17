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
            var entity = await FindGuildUserByIdAsync(user);
            if (entity != null)
                return entity;

            var guildEntity = await Context.Guilds.FirstOrDefaultAsync(o => o.Id == user.GuildId.ToString()) ?? Guild.FromDiscord(user.Guild);
            var userEntity = await Context.Users.FirstOrDefaultAsync(o => o.Id == user.Id.ToString()) ?? User.FromDiscord(user);
            entity = GuildUser.FromDiscord(user.Guild, user);
            entity.Guild = guildEntity;
            entity.User = userEntity;

            if (!Context.IsEntityTracked<Guild>(entry => entry.Entity.Id == guildEntity.Id)) await Context.AddAsync(guildEntity);
            if (!Context.IsEntityTracked<User>(entry => entry.Entity.Id == userEntity.Id)) await Context.AddAsync(userEntity);
            if (!Context.IsEntityTracked<GuildUser>(entry => entry.Entity.UserId == entity.UserId && entry.Entity.GuildId == entity.GuildId)) await Context.AddAsync(entity);

            return entity;
        }
    }

    public async Task<GuildUser?> FindGuildUserByIdAsync(IGuildUser user, bool disableTracking = false)
    {
        using (Counter.Create("Database"))
        {
            var query = Context.GuildUsers
                .Include(o => o.Guild)
                .Include(o => o.User)
                .Include(o => o.UsedInvite.Creator) // TODO Null operator in lambda queries.
                .Include(o => o.Unverify.UnverifyLog)
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

    public async Task<List<GuildUser>> GetPointsBoardDataAsync(IEnumerable<string> guildIds)
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
                .ThenBy(o => o.Nickname).ThenBy(o => o.User!.Username).ThenBy(o => o.User!.Discriminator);

            return await query.ToListAsync();
        }
    }
}
