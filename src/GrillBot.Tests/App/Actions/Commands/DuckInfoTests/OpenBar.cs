using GrillBot.Common.Services.KachnaOnline.Models;

namespace GrillBot.Tests.App.Actions.Commands.DuckInfoTests;

[TestClass]
public class OpenBar : DuckInfoTestsBase
{
    protected override DuckState State => new()
    {
        State = GrillBot.Common.Services.KachnaOnline.Enums.DuckState.OpenBar,
        Start = DateTime.Now,
        PlannedEnd = DateTime.MaxValue
    };
    
    [TestMethod]
    public override async Task RunTestAsync()
    {
        CheckEmbed(await Instance.ProcessAsync());
    }
}
