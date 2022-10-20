using GrillBot.App.Actions.Api.V1.Command;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Common.Attributes;

namespace GrillBot.Tests.App.Actions.Api.V1.Command;

[TestClass]
public class GetCommandsListTests : ApiActionTest<GetCommandsList>
{
    protected override GetCommandsList CreateAction()
    {
        var client = DiscordHelper.CreateClient();
        var interactions = DiscordHelper.CreateInteractionService(client, ServiceProvider);

        return new GetCommandsList(ApiRequestContext, interactions);
    }

    [TestMethod]
    [ApiConfiguration(canInitProvider: true)]
    public void Process()
    {
        var result = Action.Process();

        Assert.IsTrue(result.Count > 0);
        result.ForEach(o => Assert.IsTrue(o.Contains('/')));
    }
}
