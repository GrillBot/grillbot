using System.Diagnostics.CodeAnalysis;
using GrillBot.App.Actions.Api.V1.PublicApiClients;
using GrillBot.Data.Exceptions;
using GrillBot.Database.Entity;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Api.V1.PublicApiClients;

[TestClass]
public class DeleteClientTests : ApiActionTest<DeleteClient>
{
    protected override DeleteClient CreateAction()
    {
        var texts = new TextsBuilder().AddText("PublicApiClients/NotFound", "cs", "NotFound").Build();
        return new DeleteClient(ApiRequestContext, DatabaseBuilder, texts);
    }

    private async Task InitDataAsync(string id)
    {
        await Repository.AddAsync(new ApiClient { Id = id });
        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync_Success()
    {
        var id = Guid.NewGuid().ToString();
        await InitDataAsync(id);

        await Action.ProcessAsync(id);
    }

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_NotFound()
    {
        await Action.ProcessAsync(Guid.NewGuid().ToString());
    }
}
