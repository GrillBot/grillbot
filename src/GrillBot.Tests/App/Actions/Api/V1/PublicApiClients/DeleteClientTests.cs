using System.Diagnostics.CodeAnalysis;
using GrillBot.App.Actions.Api.V1.PublicApiClients;
using GrillBot.Data.Exceptions;
using GrillBot.Database.Entity;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Api.V1.PublicApiClients;

[TestClass]
public class DeleteClientTests : ApiActionTest<DeleteClient>
{
    protected override DeleteClient CreateInstance()
    {
        return new DeleteClient(ApiRequestContext, DatabaseBuilder, TestServices.Texts.Value);
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

        await Instance.ProcessAsync(id);
    }

    [TestMethod]
    [ExpectedException(typeof(NotFoundException))]
    [ExcludeFromCodeCoverage]
    public async Task ProcessAsync_NotFound()
    {
        await Instance.ProcessAsync(Guid.NewGuid().ToString());
    }
}
