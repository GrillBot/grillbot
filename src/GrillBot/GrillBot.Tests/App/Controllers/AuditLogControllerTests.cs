using GrillBot.App.Controllers;
using GrillBot.App.Services.AuditLog;
using GrillBot.Data.Models.API.AuditLog;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.Tests.App.Controllers;

[TestClass]
public class AuditLogControllerTests : ControllerTest<AuditLogController>
{
    protected override AuditLogController CreateController()
    {
        var auditLogWriter = new AuditLogWriter(DatabaseBuilder);
        var apiService = new AuditLogApiService(ApiRequestContext, auditLogWriter);

        return new AuditLogController(apiService, ServiceProvider);
    }

    [TestMethod]
    public async Task HandleClientAppMessageAsync()
    {
        var requests = new[]
        {
            new ClientLogItemRequest { IsInfo = true, Content = "Content" },
            new ClientLogItemRequest { IsWarning = true, Content = "Content" },
            new ClientLogItemRequest { IsError = true, Content = "Content" }
        };

        foreach (var request in requests)
        {
            var result = await Controller.HandleClientAppMessageAsync(request);
            CheckResult<OkResult>(result);
        }
    }
}
