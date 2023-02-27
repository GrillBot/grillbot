using GrillBot.Common.Services.KachnaOnline.Models;

namespace GrillBot.Tests.App.Actions.Commands.DuckInfoTests;

[TestClass]
public class Chillzone : DuckInfoTestsBase
{
    protected override DuckState State => new()
    {
        State = GrillBot.Common.Services.KachnaOnline.Enums.DuckState.OpenChillzone,
        PlannedEnd = DateTime.MaxValue
    };

    [TestMethod]
    public override async Task RunTestAsync()
    {
        CheckEmbed(await Instance.ProcessAsync());
    }
}
