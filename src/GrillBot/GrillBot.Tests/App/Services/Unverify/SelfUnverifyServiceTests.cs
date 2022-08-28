using GrillBot.App.Services.Unverify;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Services.Unverify;

[TestClass]
public class SelfUnverifyServiceTests : ServiceTest<SelfunverifyService>
{
    protected override SelfunverifyService CreateService()
    {
        return new SelfunverifyService(null, DatabaseBuilder);
    }

    [TestMethod]
    public async Task GetKeepablesAsync_WithSearch()
    {
        var result = await Service.GetKeepablesAsync("1bit");
        Assert.AreEqual(0, result.Count);
    }
}
