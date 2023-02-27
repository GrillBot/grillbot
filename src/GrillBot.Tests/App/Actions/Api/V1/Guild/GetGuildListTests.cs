using Discord;
using GrillBot.App.Actions.Api.V1.Guild;
using GrillBot.Data.Models.API.Guilds;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Guild;

[TestClass]
public class GetGuildListTests : ApiActionTest<GetGuildList>
{
    private IGuild Guild { get; set; }

    protected override GetGuildList CreateInstance()
    {
        Guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetGetUsersAction(Array.Empty<IGuildUser>()).Build();
        var client = new ClientBuilder().SetGetGuildsAction(new[] { Guild }).Build();

        return new GetGuildList(ApiRequestContext, DatabaseBuilder, TestServices.AutoMapper.Value, client);
    }

    [TestMethod]
    public async Task ProcessAsync_WithFilter()
    {
        var filter = new GetGuildListParams { NameQuery = "Guild" };
        var result = await Instance.ProcessAsync(filter);

        Assert.AreEqual(0, result.TotalItemsCount);
    }

    [TestMethod]
    public async Task ProcessAsync_WithoutFilter()
    {
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(Guild));
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(new GuildBuilder(Consts.GuildId + 1, Consts.GuildName).Build()));
        await Repository.CommitAsync();

        var filter = new GetGuildListParams();
        var result = await Instance.ProcessAsync(filter);

        Assert.AreEqual(2, result.TotalItemsCount);
    }
}
