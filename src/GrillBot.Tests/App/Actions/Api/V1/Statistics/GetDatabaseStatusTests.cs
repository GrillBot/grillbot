using GrillBot.App.Actions.Api.V1.Statistics;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Api.V1.Statistics;

[TestClass]
public class GetDatabaseStatusTests : ApiActionTest<GetDatabaseStatus>
{
    protected override GetDatabaseStatus CreateInstance()
    {
        return new GetDatabaseStatus(ApiRequestContext, DatabaseBuilder, CacheBuilder);
    }

    [TestMethod]
    public async Task ProcessAsync()
    {
        var result = await Instance.ProcessAsync();

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Cache);
        Assert.IsNotNull(result.Database);
    }
}
