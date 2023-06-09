using GrillBot.App.Actions;
using GrillBot.App.Actions.Api.V1.AuditLog;
using GrillBot.Common.Services.AuditLog.Models.Request.Search;
using GrillBot.Core.Models.Pagination;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.AuditLog;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/auditlog")]
[ApiExplorerSettings(GroupName = "v1")]
public class AuditLogController : Infrastructure.ControllerBase
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
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public async Task<ActionResult> RemoveItemAsync(Guid id)
    {
        await ProcessActionAsync<RemoveItem>(action => action.ProcessAsync(id));
        return Ok();
    }

    /// <summary>
    /// Get paginated list of audit logs.
    /// </summary>
    /// <response code="200">Returns paginated list of audit log items.</response>
    /// <response code="400">Validation of parameters failed.</response>
    [HttpPost("list")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public async Task<ActionResult<PaginatedResponse<LogListItem>>> SearchAuditLogsAsync([FromBody] SearchRequest request)
    {
        ApiAction.Init(this, request);
        return Ok(await ProcessActionAsync<GetAuditLogList, PaginatedResponse<LogListItem>>(action => action.ProcessAsync(request)));
    }

    /// <summary>
    /// Creates log item from client application.
    /// </summary>
    /// <response code="200"></response>
    [HttpPost("client")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
    public async Task<ActionResult> HandleClientAppMessageAsync([FromBody] ClientLogItemRequest request)
    {
        ApiAction.Init(this, request);

        await ProcessActionAsync<CreateLogItem>(action => action.ProcessAsync(request));
        return Ok();
    }
}
