using Discord;
using GrillBot.App.Actions.Api.V1.System;
using GrillBot.Common.Managers;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;

namespace GrillBot.Tests.App.Actions.Api.V1.System;

[TestClass]
public class GetDiagnosticsTests : ApiActionTest<GetDiagnostics>
{
    protected override GetDiagnostics CreateAction()
    {
        var initManager = new InitManager(TestServices.LoggerFactory.Value);
        var client = new ClientBuilder().SetConnectionState(ConnectionState.Connected).Build();

        return new GetDiagnostics(ApiRequestContext, initManager, TestServices.CounterManager.Value, TestServices.TestingEnvironment.Value, client);
    }

    [TestMethod]
    public void Process()
    {
        var result = Action.Process();

        Assert.IsNotNull(result);
        Assert.AreEqual(TestServices.TestingEnvironment.Value.EnvironmentName, result.InstanceType);
        Assert.AreEqual(ConnectionState.Connected, result.ConnectionState);
        Assert.IsFalse(result.IsActive);
    }
}
