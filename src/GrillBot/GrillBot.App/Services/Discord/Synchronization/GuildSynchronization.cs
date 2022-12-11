namespace GrillBot.App.Services.Discord.Synchronization;

public class GuildSynchronization : SynchronizationBase
{
    public GuildSynchronization(GrillBotDatabaseBuilder databaseBuilder) : base(databaseBuilder)
    {
    }

    public async Task GuildAvailableAsync(IGuild guild)
    {
        await using var repository = DatabaseBuilder.CreateRepository();

        var guildEntity = await repository.Guild.FindGuildAsync(guild);
        if (guildEntity == null) return;

        await repository.CommitAsync();
    }
}
