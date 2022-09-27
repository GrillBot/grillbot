using GrillBot.App.Actions.Api.V1.System;
using GrillBot.Common.Managers;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Api.V1.System;

[TestClass]
public class GetEventLogTests : ApiActionTest<GetEventLog>
{
    protected override GetEventLog CreateAction()
    {
        var discordClient = DiscordHelper.CreateClient();
        var interactionService = DiscordHelper.CreateInteractionService(discordClient);
        var commandService = DiscordHelper.CreateCommandsService();
        var eventManager = new EventManager(discordClient, interactionService, commandService);

        return new GetEventLog(ApiRequestContext, eventManager);
    }

    [TestMethod]
    public void Process()
    {
        var result = Action.Process();
        Assert.IsNotNull(result);
    }
}
