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
    private ApiRequest ApiRequest { get; set; } = null!;
    private ApiRequestContext ApiRequestContext { get; set; } = null!;

    protected override void PreInit()
    {
        ApiRequest = new ApiRequest();
        ApiRequestContext = new ApiRequestContext();
    }

    protected override RequestFilter CreateInstance()
    {
        var discordClient = new ClientBuilder().Build();
        var hearthbeatManager = new UserManager(DatabaseBuilder);

        return new RequestFilter(ApiRequest, ApiRequestContext, discordClient, hearthbeatManager);
    }

    [TestMethod]
    public async Task OnActionExecutionAsync()
    {
        var context = CreateContext(new HeaderDictionary());
        var @delegate = GetDelegate();

        await Instance.OnActionExecutionAsync(context, @delegate);
        Assert.AreNotEqual(DateTime.MinValue, ApiRequest.StartAt);
    }

    [TestMethod]
    public async Task OnActionExecutionAsync_InvalidModelState()
    {
        var modelState = new ModelStateDictionary();
        modelState.AddModelError("Error", "Error");

        var context = CreateContext(new HeaderDictionary(), modelState: modelState);
        var @delegate = GetDelegate();

        await Instance.OnActionExecutionAsync(context, @delegate);

        Assert.AreNotEqual(DateTime.MinValue, ApiRequest.StartAt);
        Assert.IsInstanceOfType(context.Result, typeof(BadRequestObjectResult));
    }
}
