using GrillBot.App.Controllers;
using GrillBot.App.Infrastructure.RequestProcessing;
using GrillBot.App.Managers;
using GrillBot.Data.Models.AuditLog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using GrillBot.Tests.Infrastructure.Common;
using GrillBot.Tests.Infrastructure.Discord;
using Microsoft.AspNetCore.Http;

namespace GrillBot.Tests.App.Infrastructure.RequestProcessing;

[TestClass]
public class RequestFilterTests : ActionFilterTest<RequestFilter>
{
    private ApiRequest ApiRequest { get; set; }
    private ApiRequestContext ApiRequestContext { get; set; }

    protected override bool CanInitProvider() => false;

    protected override Controller CreateController(IServiceProvider provider)
        => new AuthController(null);

    protected override RequestFilter CreateFilter()
    {
        var discordClient = new ClientBuilder().Build();

        ApiRequest = new ApiRequest();
        ApiRequestContext = new ApiRequestContext();
        var hearthbeatManager = new HearthbeatManager(DatabaseBuilder);

        return new RequestFilter(ApiRequest, ApiRequestContext, discordClient, hearthbeatManager);
    }

    [TestMethod]
    public async Task OnActionExecutionAsync()
    {
        var context = CreateContext("GetRedirectLink", new HeaderDictionary());
        var @delegate = GetDelegate();

        await Filter.OnActionExecutionAsync(context, @delegate);
        Assert.AreNotEqual(DateTime.MinValue, ApiRequest.StartAt);
    }

    [TestMethod]
    public async Task OnActionExecutionAsync_InvalidModelState()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Error", "Error");

        var context = CreateContext("GetRedirectLink", new HeaderDictionary(), modelState: modelState);
        var @delegate = GetDelegate();

        await Filter.OnActionExecutionAsync(context, @delegate);

        Assert.AreNotEqual(DateTime.MinValue, ApiRequest.StartAt);
        Assert.IsInstanceOfType(context.Result, typeof(BadRequestObjectResult));
    }
}
