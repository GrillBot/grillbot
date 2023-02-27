using Discord;
using GrillBot.App.Handlers.GuildUpdated;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Handlers.GuildUpdated;

[TestClass]
public class SyncGuildUpdatedHandlerTests : TestBase<SyncGuildUpdatedHandler>
{
    private IGuild Before { get; set; } = null!;

    protected override void PreInit()
    {
        Before = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
    }

    protected override SyncGuildUpdatedHandler CreateInstance()
    {
        return new SyncGuildUpdatedHandler(DatabaseBuilder);
    }

    private async Task InitDataAsync()
    {
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(Before));
        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync_CannotProcess()
        => await Instance.ProcessAsync(Before, Before);

    [TestMethod]
    public async Task ProcessAsync_NotFound()
    {
        var after = new GuildBuilder(Before).SetName(Consts.GuildName + "New").Build();
        await Instance.ProcessAsync(Before, after);
    }

    [TestMethod]
    public async Task ProcessAsync_Ok()
    {
        await InitDataAsync();

        var after = new GuildBuilder(Before).SetName(Consts.GuildName + "New").Build();
        await Instance.ProcessAsync(Before, after);
    }
}
