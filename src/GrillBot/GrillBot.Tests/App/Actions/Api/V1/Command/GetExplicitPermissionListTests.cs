using GrillBot.App.Actions.Api.V1.Command;
using GrillBot.Database.Enums;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.Command;

[TestClass]
public class GetExplicitPermissionListTests : ApiActionTest<GetExplicitPermissionList>
{
    protected override GetExplicitPermissionList CreateAction()
    {
        var role = new RoleBuilder(Consts.RoleId, Consts.RoleName).Build();
        var guild = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetRoles(new[] { role }).Build();
        var client = new ClientBuilder().SetGetGuildsAction(new[] { guild }).Build();

        return new GetExplicitPermissionList(ApiRequestContext, DatabaseBuilder, TestServices.AutoMapper.Value, client);
    }

    [TestMethod]
    public async Task ProcessAsync_WithoutSearch()
    {
        var user = new UserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).Build();

        await Repository.AddAsync(Database.Entity.User.FromDiscord(user));
        await Repository.AddCollectionAsync(new[]
        {
            new Database.Entity.ExplicitPermission { Command = "unverify", State = ExplicitPermissionState.Banned, IsRole = true, TargetId = Consts.RoleId.ToString() },
            new Database.Entity.ExplicitPermission { Command = "unverify", State = ExplicitPermissionState.Banned, IsRole = false, TargetId = Consts.UserId.ToString() },
        });
        await Repository.CommitAsync();

        var result = await Action.ProcessAsync(null);

        Assert.AreEqual(2, result.Count);
        result.ForEach(o => Assert.IsTrue(o.Role != null ^ o.User != null));
    }

    [TestMethod]
    public async Task ProcessAsync_WithSearch()
    {
        var result = await Action.ProcessAsync("selfunverify");
        Assert.AreEqual(0, result.Count);
    }
}
