using GrillBot.App.Controllers;
using GrillBot.App.Infrastructure.RequestProcessing;
using GrillBot.Data.Models.AuditLog;
using Microsoft.AspNetCore.Mvc;
using System;

namespace GrillBot.Tests.App.Infrastructure.RequestProcessing;

[TestClass]
public class RequestFilterTests : ActionFilterTest<RequestFilter>
{
    private ApiRequest ApiRequest { get; set; }

    protected override bool CanInitProvider() => false;

    protected override Controller CreateController(IServiceProvider provider)
        => new AuthController(null);

    protected override RequestFilter CreateFilter()
    {
        ApiRequest = new ApiRequest();
        return new RequestFilter(ApiRequest);
    }

    [TestMethod]
    public async Task OnActionExecutionAsync()
    {
        var context = CreateContext("GetRedirectLink");
        var @delegate = GetDelegate();

        await Filter.OnActionExecutionAsync(context, @delegate);

        Assert.AreNotEqual(DateTime.MinValue, ApiRequest.StartAt);
    }
}
