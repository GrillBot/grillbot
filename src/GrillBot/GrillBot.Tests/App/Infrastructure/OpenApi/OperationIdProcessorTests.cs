using GrillBot.App.Controllers;
using GrillBot.App.Infrastructure.OpenApi;
using GrillBot.Tests.Infrastructure.Common;
using NSwag;
using NSwag.Generation.Processors.Contexts;

namespace GrillBot.Tests.App.Infrastructure.OpenApi;

[TestClass]
public class OperationIdProcessorTests : ServiceTest<OperationIdProcessor>
{
    protected override OperationIdProcessor CreateService() => new();

    [TestMethod]
    public void Process()
    {
        var description = new OpenApiOperationDescription
        {
            Operation = new OpenApiOperation()
        };
        var controllerType = typeof(AuthController);
        const string methodName = "GetRedirectLink";
        var methodInfo = controllerType.GetMethod(methodName);
        
        var context = new OperationProcessorContext(null, description, controllerType, methodInfo, null, null, null, null, null);
        var result = Service.Process(context);

        Assert.IsTrue(result);
        Assert.AreEqual($"AuthController_{methodName}", context.OperationDescription.Operation.OperationId);
    }
}
