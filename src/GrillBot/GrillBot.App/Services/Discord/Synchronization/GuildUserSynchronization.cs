using GrillBot.Database.Entity;
using GrillBot.Database.Enums;
using GrillBot.Database.Extensions;

namespace GrillBot.App.Services.Discord.Synchronization;

public class GuildUserSynchronization : SynchronizationBase
{
    public GuildUserSynchronization(GrillBotContextFactory dbFactory) : base(dbFactory)
    {
    }

    private static IQueryable<GuildUser> GetBaseQuery(GrillBotContext context, ulong guildId)
        => context.GuildUsers.Include(o => o.User).Where(o => o.GuildId == guildId.ToString());

    public Task GuildMemberUpdatedAsync(IGuildUser _, IGuildUser after) => UserJoinedAsync(after);

    public async Task UserJoinedAsync(IGuildUser user)
    {
        using var context = DbFactory.Create();

        var baseQuery = GetBaseQuery(context, user.GuildId);
        var guildUser = await baseQuery.FirstOrDefaultAsync(o => o.UserId == user.Id.ToString());
        if (guildUser == null) return;

        guildUser.Nickname = user.Nickname;
        guildUser.User.Username = user.Username;
        guildUser.User.Discriminator = user.Discriminator;
        guildUser.User.Status = user.GetStatus();

        if (!user.IsUser())
            guildUser.User.Flags |= (int)UserFlags.NotUser;

        await context.SaveChangesAsync();
    }

    public async Task InitUsersAsync(IGuild guild, List<GuildUser> dbUsers)
    {
        var guildUsersQuery = dbUsers
            .Where(o => o.GuildId == guild.Id.ToString());

        foreach (var user in await guild.GetUsersAsync())
        {
            var guildUser = guildUsersQuery.FirstOrDefault(o => o.UserId == user.Id.ToString());
            if (guildUser == null) continue;

            guildUser.Nickname = user.Nickname;
            guildUser.User.Username = user.Username;
            guildUser.User.Discriminator = user.Discriminator;
            guildUser.User.Status = user.GetStatus();

            if (!user.IsUser())
                guildUser.User.Flags |= (int)UserFlags.NotUser;
        }
    }
}
