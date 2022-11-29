using GrillBot.App.Actions.Api.V1.PublicApiClients;
using GrillBot.Data.Models.API.ApiClients;
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
        var parameters = new ApiClientParams
        {
            Name = "ApiAction",
            AllowedMethods = new List<string> { "*" }
        };
        await Action.ProcessAsync(parameters);

        var clients = await Repository.ApiClientRepository.GetClientsAsync();
        Assert.IsTrue(clients.Count > 0);
    }
}
