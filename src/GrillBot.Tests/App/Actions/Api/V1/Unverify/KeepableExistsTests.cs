using GrillBot.App.Actions.Api.V1.Unverify;
using GrillBot.Data.Models.API.Selfunverify;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Api.V1.Unverify;

[TestClass]
public class KeepableExistsTests : ApiActionTest<KeepableExists>
{
    protected override KeepableExists CreateInstance()
    {
        return new KeepableExists(ApiRequestContext, DatabaseBuilder);
    }

    [TestMethod]
    public async Task ProcessAsync()
    {
        var parameters = new KeepableParams { Group = "1BIT", Name = "IZP" };
        var result = await Instance.ProcessAsync(parameters);

        Assert.IsFalse(result);
    }
}
