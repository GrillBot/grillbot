using GrillBot.App.Controllers;
using GrillBot.App.Infrastructure.RequestProcessing;
using GrillBot.App.Services.AuditLog;
using GrillBot.Cache.Services.Managers;
using GrillBot.Common.Managers;
using GrillBot.Common.Managers.Counters;
using GrillBot.Data.Models.AuditLog;
using GrillBot.Tests.Infrastructure;
using GrillBot.Tests.Infrastructure.Discord;
using Microsoft.AspNetCore.Mvc;
using System;
using GrillBot.Common.Models;

namespace GrillBot.Tests.App.Infrastructure.RequestProcessing;

[TestClass]
public class ResultFilterTests : ActionFilterTest<ResultFilter>
{
    private ApiRequest ApiRequest { get; set; }

    protected override bool CanInitProvider() => false;

    protected override Controller CreateController(IServiceProvider provider)
        => new AuthController(null);

    protected override ResultFilter CreateFilter()
    {
        ApiRequest = new ApiRequest();

        var auditLogWriter = new AuditLogWriter(DatabaseBuilder);
        return new ResultFilter(ApiRequest, auditLogWriter, new ApiRequestContext());
    }

    [TestMethod]
    public async Task OnResultExecutionAsync()
    {
        var context = GetContext(new OkResult());
        await Filter.OnResultExecutionAsync(context, GetResultDelegate());

        Assert.AreNotEqual(DateTime.MinValue, ApiRequest.EndAt);
    }
}
