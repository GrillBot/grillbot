using GrillBot.App.Actions.Api.V3.Logging;
using GrillBot.App.Actions;
using GrillBot.App.Infrastructure.Auth;
using GrillBot.Data.Models.API.AuditLog;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers;

[JwtAuthorize]
[ApiExplorerSettings(GroupName = "v3")]
public class LoggingController : Core.Infrastructure.Actions.ControllerBase
{
    public LoggingController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public Task<IActionResult> HandleFrontendLogAsync([FromBody] FrontendLogItemRequest request)
    {
        ApiAction.Init(this, request);
        return ProcessAsync<FrontendLogHandler>(request);
    }
}
