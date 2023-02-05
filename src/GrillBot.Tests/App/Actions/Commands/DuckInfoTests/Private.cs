using GrillBot.Common.Services.KachnaOnline.Models;

namespace GrillBot.Tests.App.Actions.Commands.DuckInfoTests;

[TestClass]
public class Private : DuckInfoTestsBase
{
    protected override DuckState? State => new()
    {
        State = GrillBot.Common.Services.KachnaOnline.Enums.DuckState.Private,
        FollowingState = null,
        Note = "Some note"
    };

    [TestMethod]
    public override async Task RunTestAsync()
    {
        CheckEmbed(await Action.ProcessAsync());
    }
}
