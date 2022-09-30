using GrillBot.App.Actions.Api.V1.Guild;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Common.Attributes;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Guild;

[TestClass]
public class GetRolesTests : ApiActionTest<GetRoles>
{
    protected override GetRoles CreateAction()
    {
        var role = new RoleBuilder().SetIdentity(Consts.RoleId, Consts.RoleName).Build();
        var guildBuilder = new GuildBuilder().SetIdentity(Consts.GuildId, Consts.GuildName);
        var user = new GuildUserBuilder().SetIdentity(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(guildBuilder.Build()).Build();
        var guild = guildBuilder.SetRoles(new[] { role }).SetGetUsersAction(new[] { user }).Build();

        var client = new ClientBuilder()
            .SetGetGuildsAction(new[] { guild })
            .Build();

        return new GetRoles(ApiRequestContext, client);
    }

    [TestMethod]
    [ApiConfiguration(true)]
    public async Task ProcessAsync_Public()
    {
        var result = await Action.ProcessAsync(Consts.GuildId);
        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public async Task ProcessAsync_Private()
    {
        var result = await Action.ProcessAsync(null);
        Assert.AreEqual(1, result.Count);
    }
}
