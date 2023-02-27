using GrillBot.App.Actions.Api.V1.PublicApiClients;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Api.V1.PublicApiClients;

[TestClass]
public class GetPublicApiMethodsTests : ApiActionTest<GetPublicApiMethods>
{
    protected override GetPublicApiMethods CreateInstance()
    {
        return new GetPublicApiMethods(ApiRequestContext);
    }

    [TestMethod]
    public void Process()
    {
        var result = Instance.Process();
        Assert.IsTrue(result.Count > 0);
    }
}
