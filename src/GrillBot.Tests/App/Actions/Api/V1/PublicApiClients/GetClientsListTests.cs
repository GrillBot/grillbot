using GrillBot.App.Actions.Api.V1.PublicApiClients;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Api.V1.PublicApiClients;

[TestClass]
public class GetClientsListTests : ApiActionTest<GetClientsList>
{
    protected override GetClientsList CreateAction()
    {
        return new GetClientsList(ApiRequestContext, DatabaseBuilder);
    }

    [TestMethod]
    public async Task ProcessAsync()
    {
        var result = await Action.ProcessAsync();
        Assert.AreEqual(0, result.Count);
    }
}
