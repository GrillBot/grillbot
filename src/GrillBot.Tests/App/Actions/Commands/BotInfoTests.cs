using Discord;
using GrillBot.App.Actions.Commands;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Commands;

[TestClass]
public class BotInfoTests : CommandActionTest<BotInfo>
{
    private static readonly IRole[] Roles =
    {
        new RoleBuilder(Consts.RoleId, Consts.RoleName).SetColor(Color.Default).Build(),
        new RoleBuilder(Consts.RoleId + 1, Consts.RoleName).SetColor(Color.Blue).Build(),
    };

    private static readonly GuildBuilder GuildBuilder = new GuildBuilder(Consts.GuildId, Consts.GuildName).SetRoles(Roles);

    private static readonly IGuildUser BotUser = new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).AsBot()
        .SetRoles(new[] { Consts.RoleId, Consts.RoleId + 1 }).SetGuild(GuildBuilder.Build()).Build();

    protected override IGuild Guild
        => GuildBuilder.SetGetCurrentUserAction(BotUser).Build();

    protected override IGuildUser User => BotUser;

    protected override BotInfo CreateAction()
    {
        return InitAction(new BotInfo(TestServices.Texts.Value));
    }

    [TestMethod]
    public async Task ProcessAsync()
    {
        var result = await Action.ProcessAsync();

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Color);
        Assert.AreEqual(Color.Blue, result.Color);
        Assert.AreEqual(7, result.Fields.Length);
        Assert.IsNotNull(result.Thumbnail);
        Assert.IsNotNull(result.Timestamp);
        Assert.IsNotNull(result.Footer);
        Assert.IsNotNull(result.Title);
    }
}
