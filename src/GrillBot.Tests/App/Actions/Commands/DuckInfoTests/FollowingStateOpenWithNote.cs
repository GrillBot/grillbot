using GrillBot.Common.Services.KachnaOnline.Models;

namespace GrillBot.Tests.App.Actions.Commands.DuckInfoTests;

[TestClass]
public class FollowingStateOpenWithNote : DuckInfoTestsBase
{
    protected override DuckState State => new()
    {
        State = GrillBot.Common.Services.KachnaOnline.Enums.DuckState.Closed,
        Note = "Some note",
        FollowingState = new DuckState
        {
            State = GrillBot.Common.Services.KachnaOnline.Enums.DuckState.OpenBar
        }
    };

    [TestMethod]
    public override async Task RunTestAsync()
    {
        CheckEmbed(await Action.ProcessAsync());
    }
}
