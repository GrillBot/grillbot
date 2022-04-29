namespace GrillBot.App.Services.Discord.Synchronization;

public class GuildSynchronization : SynchronizationBase
{
    public GuildSynchronization(GrillBotContextFactory dbFactory) : base(dbFactory)
    {
    }

    public Task GuildUpdatedAsync(IGuild _, IGuild after) => GuildAvailableAsync(after);

    public async Task GuildAvailableAsync(IGuild guild)
    {
        using var context = DbFactory.Create();

        var guildEntity = await context.Guilds.FirstOrDefaultAsync(x => x.Id == guild.Id.ToString());
        if (guildEntity == null) return;

        guildEntity.BoosterRoleId = guild.Roles.FirstOrDefault(o => o.Tags?.IsPremiumSubscriberRole == true)?.Id.ToString();
        guildEntity.Name = guild.Name;

        await context.SaveChangesAsync();
    }
}
