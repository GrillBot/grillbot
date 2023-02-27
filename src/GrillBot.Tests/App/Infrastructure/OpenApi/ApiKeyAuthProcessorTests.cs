using System.Linq;
using GrillBot.App.Controllers;
using GrillBot.App.Infrastructure.Auth;
using GrillBot.App.Infrastructure.OpenApi;
using GrillBot.Tests.Infrastructure.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using NSwag;
using NSwag.Generation.AspNetCore;

namespace GrillBot.Tests.App.Infrastructure.OpenApi;

[TestClass]
public class ApiKeyAuthProcessorTests : TestBase<ApiKeyAuthProcessor>
{
    protected override ApiKeyAuthProcessor CreateInstance()
    {
        return new ApiKeyAuthProcessor();
    }

    [TestMethod]
    public void Process_MissingMetaData()
        => ProcessTest<AuthController>(nameof(AuthController.CreateLoginTokenFromIdAsync), null, false);

    [TestMethod]
    public void Process_Anonymous()
    {
        var metadata = new List<object> { new AllowAnonymousAttribute() };
        ProcessTest<AuthController>(nameof(AuthController.GetRedirectLink), metadata, false);
    }

    [TestMethod]
    public void Process_Authorize()
    {
        var metadata = new List<object> { new AuthorizeAttribute() };
        ProcessTest<AuthController>(nameof(AuthController.GetRedirectLink), metadata, false);
    }

    [TestMethod]
    public void Process_MissingApiKeyAttribute()
    {
        var metadata = new List<object>();
        ProcessTest<AuthController>(nameof(AuthController.GetRedirectLink), metadata, false);
    }

    [TestMethod]
    public void Process_Finish()
    {
        var metadata = new List<object> { new ApiKeyAuthAttribute() };
        ProcessTest<AuthController>(nameof(AuthController.GetRedirectLink), metadata, true);
    }

    private void ProcessTest<TController>(string methodName, IList<object>? endpointMetadata, bool checkApiKey) where TController : Controller
    {
        var controllerType = typeof(TController);
        var operationDescription = new OpenApiOperationDescription { Operation = new OpenApiOperation() };
        var methodInfo = controllerType.GetMethod(methodName);
        var context = new AspNetCoreOperationProcessorContext(null, operationDescription, controllerType, methodInfo, null, null, null, null, null)
        {
            ApiDescription = new ApiDescription
            {
                ActionDescriptor = new ControllerActionDescriptor
                {
                    EndpointMetadata = endpointMetadata ?? new List<object>()
                }
            }
        };

        var result = Instance.Process(context);

        Assert.IsTrue(result);
        if (!checkApiKey)
            return;

        Assert.IsNotNull(operationDescription.Operation.Security);
        Assert.AreEqual(1, operationDescription.Operation.Security.Count);
        Assert.AreEqual("ApiKey", operationDescription.Operation.Security.First().Keys.First());
    }
}
