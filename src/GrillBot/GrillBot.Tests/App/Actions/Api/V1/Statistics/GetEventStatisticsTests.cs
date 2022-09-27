using GrillBot.App.Actions.Api.V1.Statistics;
using GrillBot.Common.Managers;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Api.V1.Statistics;

[TestClass]
public class GetEventStatisticsTests : ApiActionTest<GetEventStatistics>
{
    protected override GetEventStatistics CreateAction()
    {
        var discordClient = DiscordHelper.CreateClient();
        var interactionService = DiscordHelper.CreateInteractionService(discordClient);
        var commandService = DiscordHelper.CreateCommandsService();

        var eventManager = new EventManager(discordClient, interactionService, commandService);
        return new GetEventStatistics(ApiRequestContext, eventManager);
    }

    [TestMethod]
    public void Process()
    {
        var result = Action.Process();
        Assert.AreEqual(0, result.Count);
    }
}
