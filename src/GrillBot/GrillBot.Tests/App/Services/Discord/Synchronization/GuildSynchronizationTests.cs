using GrillBot.App.Services.Discord.Synchronization;
using GrillBot.Database.Entity;

namespace GrillBot.Tests.App.Services.Discord.Synchronization;

[TestClass]
public class GuildSynchronizationTests : ServiceTest<GuildSynchronization>
{
    protected override GuildSynchronization CreateService()
    {
        return new GuildSynchronization(DbFactory);
    }

    [TestMethod]
    public async Task GuildUpdatedAsync_GuildNotFound()
    {
        var guild = DataHelper.CreateGuild();

        await Service.GuildUpdatedAsync(null, guild);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task GuildUpdatedAsync_Ok()
    {
        var guild = DataHelper.CreateGuild();

        await DbContext.Guilds.AddAsync(Guild.FromDiscord(guild));
        await DbContext.SaveChangesAsync();

        await Service.GuildUpdatedAsync(guild, guild);
        Assert.IsTrue(true);
    }
}
