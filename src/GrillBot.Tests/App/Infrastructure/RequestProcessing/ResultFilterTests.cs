using GrillBot.App.Controllers;
using GrillBot.App.Infrastructure.RequestProcessing;
using GrillBot.App.Managers;
using GrillBot.Data.Models.AuditLog;
using Microsoft.AspNetCore.Mvc;
using GrillBot.Tests.Infrastructure.Common;

namespace GrillBot.Tests.App.Infrastructure.RequestProcessing;

[TestClass]
public class ResultFilterTests : ActionFilterTest<ResultFilter>
{
    private ApiRequest ApiRequest { get; set; } = null!;

    protected override void PreInit()
    {
        ApiRequest = new ApiRequest();
    }

    protected override ResultFilter CreateInstance()
    {
        var auditLogWriter = new AuditLogWriteManager(DatabaseBuilder);
        return new ResultFilter(ApiRequest, auditLogWriter, new ApiRequestContext());
    }

    [TestMethod]
    public async Task OnResultExecutionAsync()
    {
        var context = GetContext(new OkResult());
        await Instance.OnResultExecutionAsync(context, GetResultDelegate());

        Assert.AreNotEqual(DateTime.MinValue, ApiRequest.EndAt);
    }
}
