using GrillBot.App.Services.AuditLog;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Extensions;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.AuditLog;
using GrillBot.Data.Models.API.AuditLog.Filters;
using GrillBot.Database.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using NSwag.Annotations;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/auditlog")]
[OpenApiTag("Audit log", Description = "Logging")]
public class AuditLogController : Controller
{
    private AuditLogApiService ApiService { get; }

    public AuditLogController(AuditLogApiService apiService)
    {
        ApiService = apiService;
    }

    /// <summary>
    /// Removes item from log.
    /// </summary>
    /// <param name="id">Log item ID</param>
    /// <response code="200"></response>
    /// <response code="404">Item not found.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public async Task<ActionResult> RemoveItemAsync(long id)
    {
        var result = await ApiService.RemoveItemAsync(id);

        if (!result)
            return NotFound(new MessageResponse("Požadovaný záznam v logu nebyl nalezen nebo nemáš oprávnění přistoupit k tomuto záznamu."));

        return Ok();
    }

    /// <summary>
    /// Gets paginated list of audit logs.
    /// </summary>
    /// <response code="200">Returns paginated list of audit log items.</response>
    /// <response code="400">Validation of parameters failed.</response>
    [HttpGet]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public async Task<ActionResult<PaginatedResponse<AuditLogListItem>>> GetAuditLogListAsync([FromQuery] AuditLogListParams parameters)
    {
        var result = await ApiService.GetListAsync(parameters);
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
    public async Task<ActionResult> HandleClientAppMessageAsync(ClientLogItemRequest request)
    {
        this.SetApiRequestData(request);
        await ApiService.HandleClientAppMessageAsync(request);
        return Ok();
    }
}
