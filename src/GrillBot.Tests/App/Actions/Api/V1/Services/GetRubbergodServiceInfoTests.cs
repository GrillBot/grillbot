using GrillBot.App.Actions.Api.V1.Services;
using GrillBot.Common.Managers.Logging;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Api.V1.Services;

[TestClass]
public class GetRubbergodServiceInfoTests : ApiActionTest<GetRubbergodServiceInfo>
{
    protected override GetRubbergodServiceInfo CreateInstance()
    {
        var client = TestServices.DiscordSocketClient.Value;
        var interaction = DiscordHelper.CreateInteractionService(client);
        var logging = new LoggingManager(client, interaction, TestServices.Provider.Value);
        return new GetRubbergodServiceInfo(ApiRequestContext, TestServices.RubbergodServiceClient.Value, logging);
    }

    [TestMethod]
    public async Task ProcessAsync()
    {
        var result = await Instance.ProcessAsync();

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Info);
    }
}
