using Discord;
using GrillBot.App.Handlers.GuildUpdated;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Handlers.GuildUpdated;

[TestClass]
public class SyncGuildUpdatedHandlerTests : HandlerTest<SyncGuildUpdatedHandler>
{
    private IGuild Before { get; set; }

    protected override SyncGuildUpdatedHandler CreateHandler()
    {
        Before = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();

        return new SyncGuildUpdatedHandler(DatabaseBuilder);
    }

    private async Task InitDataAsync()
    {
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(Before));
        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync_CannotProcess()
    {
        await Handler.ProcessAsync(Before, Before);
    }

    [TestMethod]
    public async Task ProcessAsync_NotFound()
    {
        var after = new GuildBuilder(Before).SetName(Consts.GuildName + "New").Build();
        await Handler.ProcessAsync(Before, after);
    }

    [TestMethod]
    public async Task ProcessAsync_Ok()
    {
        await InitDataAsync();

        var after = new GuildBuilder(Before).SetName(Consts.GuildName + "New").Build();
        await Handler.ProcessAsync(Before, after);
    }
}
