using GrillBot.App.Services.Unverify;

namespace GrillBot.Tests.App.Services.Unverify;

[TestClass]
public class SelfUnverifyServiceTests : ServiceTest<SelfunverifyService>
{
    protected override SelfunverifyService CreateService()
    {
        var dbFactory = new DbContextBuilder();
        DbContext = dbFactory.Create();

        return new SelfunverifyService(null, dbFactory);
    }

    public override void Cleanup()
    {
        DbContext.SelfunverifyKeepables.RemoveRange(DbContext.SelfunverifyKeepables);
        DbContext.SaveChanges();
    }

    [TestMethod]
    public async Task GetKeepablesAsync_WithSearch()
    {
        var result = await Service.GetKeepablesAsync("1bit");
        Assert.AreEqual(0, result.Count);
    }
}
