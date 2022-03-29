using GrillBot.App.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class DocsControllerTests : ControllerTest<DocsController>
{
    protected override DocsController CreateController()
    {
        return new DocsController();
    }

    [TestMethod]
    public void GetNamespaceGraph()
    {
        var result = Controller.GetNamespaceGraph();
        CheckResult<OkObjectResult, string>(result);
    }
}
