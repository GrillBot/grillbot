using System.Diagnostics.CodeAnalysis;
using GrillBot.App.Actions.Api.V1.PublicApiClients;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.API.ApiClients;
using GrillBot.Database.Entity;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Api.V1.PublicApiClients;

[TestClass]
public class UpdateClientTests : ApiActionTest<UpdateClient>
{
    protected override UpdateClient CreateAction()
    {
        var texts = new TextsBuilder().AddText("PublicApiClients/NotFound", "cs", "NotFound").Build();
        return new UpdateClient(ApiRequestContext, DatabaseBuilder, texts);
    }

    private async Task InitDataAsync(string id)
    {
        await Repository.AddAsync(new ApiClient { Id = id, Name = "Name" });
        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync_Success()
    {
        var id = Guid.NewGuid().ToString();
        await InitDataAsync(id);

        var parameters = new ApiClientParams
        {
            Name = "ApiAction",
            AllowedMethods = new List<string> { "*" }
        };
        await Action.ProcessAsync(id, parameters);
    }

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_NotFound()
    {
        await Action.ProcessAsync(Guid.NewGuid().ToString(), new ApiClientParams());
    }
}
