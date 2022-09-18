using GrillBot.App.Actions;
using GrillBot.App.Services.AuditLog;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.AuditLog;
using GrillBot.Data.Models.API.AuditLog.Filters;
using GrillBot.Database.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/auditlog")]
[ApiExplorerSettings(GroupName = "v1")]
public class AuditLogController : Controller
{
    private AuditLogApiService ApiService { get; }
    private IServiceProvider ServiceProvider { get; }

    public AuditLogController(AuditLogApiService apiService, IServiceProvider serviceProvider)
    {
        ApiService = apiService;
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
        var result = await action.ProcessAsync(id);

        if (result != null)
            return StatusCode(result.Value.status, new MessageResponse(result.Value.response));

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
        try
        {
            var fileInfo = await ApiService.GetLogItemFileAsync(id, fileId);
            var contentType = new FileExtensionContentTypeProvider().TryGetContentType(fileInfo.FullName, out var type) ? type : "application/octet-stream";

            var content = await System.IO.File.ReadAllBytesAsync(fileInfo.FullName);
            return File(content, contentType);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new MessageResponse(ex.Message));
        }
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
        this.StoreParameters(request);

        await ApiService.HandleClientAppMessageAsync(request);
        return Ok();
    }
}
