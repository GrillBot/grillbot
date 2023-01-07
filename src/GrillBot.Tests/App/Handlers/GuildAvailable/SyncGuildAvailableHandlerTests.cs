using Discord;
using GrillBot.App.Handlers.GuildAvailable;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Handlers.GuildAvailable;

[TestClass]
public class SyncGuildAvailableHandlerTests : HandlerTest<SyncGuildAvailableHandler>
{
    private IGuild Guild { get; set; }

    protected override SyncGuildAvailableHandler CreateHandler()
    {
        Guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();

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
        await Handler.ProcessAsync(Guild);
    }

    [TestMethod]
    public async Task ProcessAsync_Ok()
    {
        await InitDataAsync();
        await Handler.ProcessAsync(Guild);
    }
}
