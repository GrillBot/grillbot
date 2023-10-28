using GrillBot.App.Actions;
using GrillBot.App.Actions.Api.V1.AuditLog;
using GrillBot.App.Actions.Api.V2.AuditLog;
using GrillBot.App.Infrastructure.Auth;
using GrillBot.Core.Services.AuditLog.Models.Request.Search;
using GrillBot.Core.Services.AuditLog.Models.Response.Detail;
using GrillBot.Core.Models.Pagination;
using GrillBot.Data.Models.API;
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
    /// Remove item from audit log.
    /// </summary>
    /// <response code="200">Success.</response>
    /// <response code="404">Item not found.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public async Task<IActionResult> RemoveItemAsync(Guid id)
        => await ProcessAsync<RemoveItem>(id);

    /// <summary>
    /// Get paginated list of audit logs.
    /// </summary>
    /// <response code="200">Returns paginated list of audit log items.</response>
    /// <response code="400">Validation of parameters failed.</response>
    [HttpPost("list")]
    [ProducesResponseType(typeof(PaginatedResponse<LogListItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public async Task<IActionResult> SearchAuditLogsAsync([FromBody] SearchRequest request)
    {
        ApiAction.Init(this, request);
        return await ProcessAsync<GetAuditLogList>(request);
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
    /// Get detailed information of log item.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Detail), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public async Task<IActionResult> DetailAsync(Guid id)
        => await ProcessAsync<GetAuditLogDetail>(id);

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
