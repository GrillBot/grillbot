using GrillBot.Database.Entity;

namespace GrillBot.App.Services.Discord.Synchronization;

public class GuildUserSynchronization : SynchronizationBase
{
    public GuildUserSynchronization(GrillBotDatabaseBuilder databaseBuilder) : base(databaseBuilder)
    {
    }

    public Task GuildMemberUpdatedAsync(IGuildUser after) => UserJoinedAsync(after);

    public async Task UserJoinedAsync(IGuildUser user)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var guildUser = await repository.GuildUser.FindGuildUserAsync(user);
        if (guildUser == null) return;

        await repository.CommitAsync();
    }

    public static async Task InitUsersAsync(IGuild guild, List<GuildUser> dbUsers)
    {
        var guildUsers = dbUsers.FindAll(o => o.GuildId == guild.Id.ToString());

        foreach (var user in await guild.GetUsersAsync())
        {
            var guildUser = guildUsers.Find(o => o.UserId == user.Id.ToString());
            guildUser?.Update(user);
        }
    }
}
