using System;
using GrillBot.App.Controllers;
using GrillBot.App.Infrastructure.OpenApi;
using NSwag.Generation.Processors.Contexts;

namespace GrillBot.Tests.App.Infrastructure.OpenApi;

[TestClass]
public class OnlyDevelopmentProcessorTests : ServiceTest<OnlyDevelopmentProcessor>
{
    protected override OnlyDevelopmentProcessor CreateService() => new();

    [TestMethod]
    public void Process_NotSet()
    {
        var controllerType = typeof(AuthController);
        var methodInfo = controllerType.GetMethod("GetRedirectLink");
        var context = new OperationProcessorContext(null, null, controllerType, methodInfo, null, null, null, null, null);
        var result = Service.Process(context);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Process_Development()
    {
        try
        {
            var controllerType = typeof(AuthController);
            var methodInfo = controllerType.GetMethod(nameof(AuthController.CreateLoginTokenFromIdAsync));
            var context = new OperationProcessorContext(null, null, controllerType, methodInfo, null, null, null, null, null);
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
            var result = Service.Process(context);

            Assert.IsTrue(result);
        }
        finally
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
        }
    }

    [TestMethod]
    public void Process_Production()
    {
        try
        {
            var controllerType = typeof(AuthController);
            var methodInfo = controllerType.GetMethod(nameof(AuthController.CreateLoginTokenFromIdAsync));
            var context = new OperationProcessorContext(null, null, controllerType, methodInfo, null, null, null, null, null);
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
            var result = Service.Process(context);

            Assert.IsFalse(result);
        }
        finally
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
        }
    }
}
