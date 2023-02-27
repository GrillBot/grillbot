using GrillBot.App.Actions.Api.V1.System;
using GrillBot.Common.Managers;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Api.V1.System;

[TestClass]
public class ChangeBotStatusTests : ApiActionTest<ChangeBotStatus>
{
    private InitManager InitManager { get; set; }

    protected override ChangeBotStatus CreateInstance()
    {
        InitManager = new InitManager(TestServices.LoggerFactory.Value);

        return new ChangeBotStatus(ApiRequestContext, InitManager);
    }

    [TestMethod]
    public void Process()
    {
        Instance.Process(true);
        Assert.IsTrue(InitManager.Get());
    }
}
