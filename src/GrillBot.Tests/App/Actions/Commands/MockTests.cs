using GrillBot.App.Actions.Commands;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Commands;

[TestClass]
public class MockTests : CommandActionTest<Mock>
{
    protected override Mock CreateInstance()
    {
        return InitAction(new Mock(TestServices.Configuration.Value, TestServices.Random.Value));
    }

    [TestMethod]
    public void Process()
    {
        var result = Instance.Process("This Is test");

        Assert.IsTrue(result.StartsWith("<a:mocking"));
        Assert.IsTrue(result.EndsWith(">"));
    }

    [TestMethod]
    public void Process_Mocked()
    {
        const string input = "ThisIsTest";

        var result = Instance.Process(input);
        var nextResult = Instance.Process(result);

        Assert.AreNotEqual(input, result);
        Assert.AreNotEqual(input, nextResult);
        Assert.AreEqual(result, nextResult);
    }
}
