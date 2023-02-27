using System.Linq;
using GrillBot.App.Actions.Api.V1.Unverify;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Api.V1.Unverify;

[TestClass]
public class GetKeepableListTests : ApiActionTest<GetKeepablesList>
{
    protected override GetKeepablesList CreateInstance()
    {
        return new GetKeepablesList(ApiRequestContext, DatabaseBuilder);
    }

    private async Task InitDataAsync()
    {
        await Repository.AddAsync(new Database.Entity.SelfunverifyKeepable { Name = "izp", GroupName = "1bit" });
        await Repository.CommitAsync();
    }

    [TestMethod]
    public async Task ProcessAsync_WithoutGroup()
    {
        await InitDataAsync();

        var result = await Instance.ProcessAsync(null);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("1BIT", result.Keys.First());
    }

    [TestMethod]
    public async Task ProcessAsync_WithGroup()
    {
        await InitDataAsync();

        var result = await Instance.ProcessAsync("2BIT");
        Assert.AreEqual(0, result.Count);
    }
}
