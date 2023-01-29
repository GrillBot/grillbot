using Discord.WebSocket;
using GrillBot.App.Actions.Api.V1.Services;
using GrillBot.Common.Managers.Logging;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Api.V1.Services;

[TestClass]
public class GetGraphicsServiceInfoTests : ApiActionTest<GetGraphicsServiceInfo>
{
    protected override GetGraphicsServiceInfo CreateAction()
    {
        var client = new DiscordSocketClient();
        var interactionService = DiscordHelper.CreateInteractionService(client);
        var logging = new LoggingManager(client, interactionService, TestServices.Provider.Value);
        return new GetGraphicsServiceInfo(ApiRequestContext, TestServices.Graphics.Value, logging);
    }

    [TestMethod]
    public async Task ProcessAsync_Success()
    {
        var result = await Action.ProcessAsync();
        
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Metrics);
        Assert.IsNotNull(result.Statistics);
        Assert.IsNotNull(result.Version);
    }
}
