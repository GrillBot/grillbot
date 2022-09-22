using Discord;
using GrillBot.App.Actions.Api.V1.Guild;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Guild;

[TestClass]
public class GetAvailableGuildsTests : ApiActionTest<GetAvailableGuilds>
{
    private IGuild Guild { get; set; }

    protected override GetAvailableGuilds CreateAction()
    {
        var user = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).Build();
        Guild = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName).SetGetUserAction(user).Build();

        var client = new ClientBuilder()
            .SetGetGuildsAction(new[] { Guild })
            .Build();

        return new GetAvailableGuilds(ApiRequestContext, DatabaseBuilder, client);
    }

    [TestMethod]
    [ControllerTestConfiguration(true)]
    public async Task ProcessAsync_AsUser()
    {
        var result = await Action.ProcessAsync();
        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task ProcessAsync_AsAdmin()
    {
        await Repository.AddAsync(Database.Entity.Guild.FromDiscord(Guild));
        await Repository.CommitAsync();

        var result = await Action.ProcessAsync();
        Assert.AreEqual(1, result.Count);
    }
}
