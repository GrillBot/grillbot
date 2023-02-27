using Discord;
using GrillBot.App.Handlers.GuildAvailable;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Handlers.GuildAvailable;

[TestClass]
public class SyncGuildAvailableHandlerTests : TestBase<SyncGuildAvailableHandler>
{
    private IGuild Guild { get; set; } = null!;

    protected override void PreInit()
    {
        Guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();
    }

    protected override SyncGuildAvailableHandler CreateInstance()
    {
        return new SyncGuildAvailableHandler(DatabaseBuilder);
    }

    private async Task InitDataAsync()
    {
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(Guild));
        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync_NotFound()
    {
        await Instance.ProcessAsync(Guild);
    }

    [TestMethod]
    public async Task ProcessAsync_Ok()
    {
        await InitDataAsync();
        await Instance.ProcessAsync(Guild);
    }
}
