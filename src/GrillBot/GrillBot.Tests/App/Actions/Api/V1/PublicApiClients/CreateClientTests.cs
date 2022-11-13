using GrillBot.App.Actions.Api.V1.PublicApiClients;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Api.V1.PublicApiClients;

[TestClass]
public class CreateClientTests : ApiActionTest<CreateClient>
{
    protected override CreateClient CreateAction()
    {
        return new CreateClient(ApiRequestContext, DatabaseBuilder);
    }

    [TestMethod]
    public async Task ProcessAsync()
    {
        await Action.ProcessAsync(new List<string> { "*" });

        var clients = await Repository.ApiClientRepository.GetClientsAsync();
        Assert.IsTrue(clients.Count > 0);
    }
}
