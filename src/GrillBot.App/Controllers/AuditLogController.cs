using GrillBot.App.Actions;
using GrillBot.App.Actions.Api.V1.AuditLog;
using GrillBot.App.Actions.Api.V2.AuditLog;
using GrillBot.App.Infrastructure.Auth;
using GrillBot.Data.Models.API.AuditLog;
using GrillBot.Data.Models.API.AuditLog.Public;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/auditlog")]
[ApiExplorerSettings(GroupName = "v1")]
public class AuditLogController : Core.Infrastructure.Actions.ControllerBase
{
    public AuditLogController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <summary>
    /// Creates log item from client application.
    /// </summary>
    /// <response code="200"></response>
    [HttpPost("client")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
    public async Task<IActionResult> HandleClientAppMessageAsync([FromBody] ClientLogItemRequest request)
    {
        ApiAction.Init(this, request);
        return await ProcessAsync<CreateLogItem>(request);
    }

    /// <summary>
    /// Create text based log item.
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="400">Validation failed.</response>
    [HttpPost("create/message")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ApiKeyAuth]
    [ApiExplorerSettings(GroupName = "v2")]
    public async Task<IActionResult> CreateMessageLogItemAsync(LogMessageRequest request)
    {
        ApiAction.Init(this, request);
        return await ProcessAsync<CreateAuditLogMessageAction>(request);
    }
}
