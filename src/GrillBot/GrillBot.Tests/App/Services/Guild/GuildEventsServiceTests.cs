using GrillBot.App.Services.Guild;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Services.Guild;

[TestClass]
public class GuildEventsServiceTests : ServiceTest<GuildEventsService>
{
    protected override GuildEventsService CreateService()
    {
        return new GuildEventsService(DatabaseBuilder);
    }

    [TestMethod]
    public async Task ExistsValidGuildEventAsync_GuildNotFound()
    {
        var guild = new GuildBuilder()
            .SetIdentity(Consts.GuildId, Consts.GuildName)
            .Build();

        var result = await Service.ExistsValidGuildEventAsync(guild, "Event");
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task ExistsValidGuildEventAsync_EventNotFound()
    {
        var guild = new GuildBuilder()
            .SetIdentity(Consts.GuildId, Consts.GuildName)
            .Build();

        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(guild));
        await Repository.CommitAsync();

        var result = await Service.ExistsValidGuildEventAsync(guild, "Event");
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task ExistsValidGuildEventAsync_InvalidDate()
    {
        var guild = new GuildBuilder()
            .SetIdentity(Consts.GuildId, Consts.GuildName)
            .Build();

        var guildEntity = Database.Entity.Guild.FromDiscord(guild);
        guildEntity.GuildEvents.Add(new Database.Entity.GuildEvent
        {
            From = DateTime.MinValue,
            To = DateTime.MinValue,
            Id = "Event"
        });

        await Repository.AddAsync(guildEntity);
        await Repository.CommitAsync();

        var result = await Service.ExistsValidGuildEventAsync(guild, "Event");
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task ExistsValidGuildEventAsync_Success()
    {
        var guild = new GuildBuilder()
            .SetIdentity(Consts.GuildId, Consts.GuildName)
            .Build();

        var guildEntity = Database.Entity.Guild.FromDiscord(guild);
        guildEntity.GuildEvents.Add(new Database.Entity.GuildEvent
        {
            From = DateTime.MinValue,
            To = DateTime.MaxValue,
            Id = "Event"
        });

        await Repository.AddAsync(guildEntity);
        await Repository.CommitAsync();

        var result = await Service.ExistsValidGuildEventAsync(guild, "Event");
        Assert.IsTrue(result);
    }
}
