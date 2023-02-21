using GrillBot.App.Actions.Api.V1.Command;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Api.V1.Command;

[TestClass]
public class GetExternalCommandsTests : ApiActionTest<GetExternalCommands>
{
    protected override GetExternalCommands CreateAction()
    {
        return new GetExternalCommands(ApiRequestContext, TestServices.RubbergodServiceClient.Value);
    }

    [TestMethod]
    public async Task ProcessAsync()
    {
        var result = await Action.ProcessAsync("Rubbergod");

        Assert.IsNotNull(result);
        Assert.IsTrue(result.Count > 0);
    }

    [TestMethod]
    public async Task ProcessAsync_NoData()
    {
        var result = await Action.ProcessAsync("Rubbergod-Help-Test");

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }
}
