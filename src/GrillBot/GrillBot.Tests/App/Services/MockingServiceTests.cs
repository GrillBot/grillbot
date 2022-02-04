using GrillBot.App.Services;
using GrillBot.Tests.TestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GrillBot.Tests.App.Services;

[TestClass]
public class MockingServiceTests : ServiceTest<MockingService>
{
    protected override MockingService CreateService()
    {
        var configuration = ConfigurationHelper.CreateConfiguration();
        var randomization = new RandomizationService();

        return new MockingService(configuration, randomization);
    }

    [TestMethod]
    public void CreateMockingString()
    {
        var result = Service.CreateMockingString("This Is lest");

        Assert.IsTrue(result.StartsWith("<a:mocking"));
        Assert.IsTrue(result.EndsWith(">"));
    }
}
