using Discord;
using GrillBot.App.Actions.Commands.Searching;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Commands.Searching;

[TestClass]
public class CreateSearchTests : CommandActionTest<CreateSearch>
{
    private static readonly IGuild GuildData = new GuildBuilder(Consts.GuildId, Consts.GuildName).Build();

    protected override IGuild Guild => GuildData;
    protected override IMessageChannel Channel => new TextChannelBuilder(Consts.ChannelId, Consts.ChannelName).SetGuild(GuildData).Build();
    protected override IGuildUser User => new GuildUserBuilder(Consts.UserId, Consts.Username, Consts.Discriminator).SetGuild(GuildData).Build();

    protected override CreateSearch CreateAction()
    {
        return InitAction(new CreateSearch(DatabaseBuilder));
    }

    [TestMethod]
    public async Task ProcessAsync()
    {
        await Action.ProcessAsync("Test");
    }
}
