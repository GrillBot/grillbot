using GrillBot.App.Services.Discord.Synchronization;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Services.Discord.Synchronization;

[TestClass]
public class GuildSynchronizationTests : ServiceTest<GuildSynchronization>
{
    protected override GuildSynchronization CreateService()
    {
        return new GuildSynchronization(DatabaseBuilder);
    }

    [TestMethod]
    public async Task GuildUpdatedAsync_GuildNotFound()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();

        await Service.GuildUpdatedAsync(guild);
        Assert.IsTrue(true);
    }

    [TestMethod]
    public async Task GuildUpdatedAsync_Ok()
    {
        var guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).Build();

        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(guild));
        await Repository.CommitAsync();

        await Service.GuildUpdatedAsync(guild);
        Assert.IsTrue(true);
    }
}
