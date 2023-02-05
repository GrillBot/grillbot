using GrillBot.Common.Services.KachnaOnline.Models;

namespace GrillBot.Tests.App.Actions.Commands.DuckInfoTests;

[TestClass]
public class ClosedFollowingStateOpen : DuckInfoTestsBase
{
    protected override DuckState State => new()
    {
        State = GrillBot.Common.Services.KachnaOnline.Enums.DuckState.Closed,
        FollowingState = new DuckState
        {
            State = GrillBot.Common.Services.KachnaOnline.Enums.DuckState.OpenBar,
            PlannedEnd = DateTime.Now
        }
    };
    
    [TestMethod]
    public override async Task RunTestAsync()
    {
        CheckEmbed(await Action.ProcessAsync());
    }
}
