using GrillBot.App.Services.Unverify;

namespace GrillBot.Tests.App.Services.Unverify;

[TestClass]
public class SelfUnverifyServiceTests : ServiceTest<SelfunverifyService>
{
    protected override SelfunverifyService CreateService()
    {
        return new SelfunverifyService(null, DbFactory);
    }

    [TestMethod]
    public async Task GetKeepablesAsync_WithSearch()
    {
        var result = await Service.GetKeepablesAsync("1bit");
        Assert.AreEqual(0, result.Count);
    }
}
