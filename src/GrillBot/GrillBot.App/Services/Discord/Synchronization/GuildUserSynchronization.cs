namespace GrillBot.App.Services.Discord.Synchronization;

public class GuildUserSynchronization : SynchronizationBase
{
    public GuildUserSynchronization(GrillBotDatabaseBuilder databaseBuilder) : base(databaseBuilder)
    {
    }

    public async Task UserJoinedAsync(IGuildUser user)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var guildUser = await repository.GuildUser.FindGuildUserAsync(user);
        if (guildUser == null) return;

        await repository.CommitAsync();
    }
}
