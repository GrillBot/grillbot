using GrillBot.App.Actions.Api.V2;
using GrillBot.Data.Models.API.Users;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Api.V2;

[TestClass]
public class GetRubbergodUserKarmaTests : ApiActionTest<GetRubbergodUserKarma>
{
    protected override GetRubbergodUserKarma CreateInstance()
    {
        return new GetRubbergodUserKarma(ApiRequestContext, TestServices.RubbergodServiceClient.Value);
    }

    [TestMethod]
    public async Task ProcessAsync_Success()
    {
        var parameters = new KarmaListParams { Pagination = { Page = 2 } };
        var result = await Instance.ProcessAsync(parameters);

        Assert.IsNotNull(result.Data);
        Assert.AreEqual(1, result.Data.Count);
    }
}
