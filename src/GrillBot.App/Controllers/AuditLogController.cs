using GrillBot.App.Actions;
using GrillBot.Common.Models.Pagination;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.AuditLog;
using GrillBot.Data.Models.API.AuditLog.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/auditlog")]
[ApiExplorerSettings(GroupName = "v1")]
public class AuditLogController : Controller
{
    private IServiceProvider ServiceProvider { get; }

    public AuditLogController(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Remove item from audit log.
    /// </summary>
    /// <response code="200">Success.</response>
    /// <response code="404">Item not found.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public async Task<ActionResult> RemoveItemAsync(long id)
    {
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.AuditLog.RemoveItem>();
        await action.ProcessAsync(id);

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
    public async Task<ActionResult<PaginatedResponse<AuditLogListItem>>> GetAuditLogListAsync([FromBody] AuditLogListParams parameters)
    {
        ApiAction.Init(this, parameters);

        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.AuditLog.GetAuditLogList>();
        var result = await action.ProcessAsync(parameters);
        return Ok(result);
    }

    /// <summary>
    /// Gets file that stored in log.
    /// </summary>
    /// <response code="200">Success.</response>
    /// <response code="404">Item not found or file not exists.</response>
    [HttpGet("{id}/{fileId}")]
    [ProducesResponseType(typeof(FileContentResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public async Task<IActionResult> GetFileContentAsync(long id, long fileId)
    {
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.AuditLog.GetFileContent>();
        var result = await action.ProcessAsync(id, fileId);

        return File(result.content, result.contentType);
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

        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.AuditLog.CreateLogItem>();
        await action.ProcessAsync(request);

        return Ok();
    }
}
