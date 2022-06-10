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

    public async Task<GuildUser> GetOrCreateGuildUserAsync(IGuildUser user)
    {
        using (Counter.Create("Database"))
        {
            var entity = await Context.GuildUsers
                .FirstOrDefaultAsync(o => o.UserId == user.Id.ToString() && o.GuildId == user.GuildId.ToString());

            if (entity != null)
                return entity;

            var guildEntity = await Context.Guilds.FirstOrDefaultAsync(o => o.Id == user.GuildId.ToString()) ?? Guild.FromDiscord(user.Guild);
            var userEntity = await Context.Users.FirstOrDefaultAsync(o => o.Id == user.Id.ToString()) ?? User.FromDiscord(user);

            entity = GuildUser.FromDiscord(user.Guild, user);
            entity.Guild = guildEntity;
            entity.User = userEntity;

            await Context.AddAsync(entity);
            return entity;
        }
    }
}
