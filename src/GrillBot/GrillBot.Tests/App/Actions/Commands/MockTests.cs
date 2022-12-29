using GrillBot.App.Actions.Commands;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Actions.Commands;

[TestClass]
public class MockTests : CommandActionTest<Mock>
{
    protected override Mock CreateAction()
    {
        return InitAction(new Mock(TestServices.Configuration.Value, TestServices.Random.Value));
    }

    [TestMethod]
    public void Process()
    {
        var result = Action.Process("This Is test");

        Assert.IsTrue(result.StartsWith("<a:mocking"));
        Assert.IsTrue(result.EndsWith(">"));
    }

    [TestMethod]
    public void Process_Mocked()
    {
        const string input = "ThisIsTest";

        var result = Action.Process(input);
        var nextResult = Action.Process(result);

        Assert.AreNotEqual(input, result);
        Assert.AreNotEqual(input, nextResult);
        Assert.AreEqual(result, nextResult);
    }
}
