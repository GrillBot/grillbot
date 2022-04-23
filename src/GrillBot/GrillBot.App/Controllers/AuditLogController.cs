using GrillBot.App.Services.AuditLog;
using GrillBot.Data.Exceptions;
using GrillBot.Data.Models.API;
using GrillBot.Data.Models.API.AuditLog;
using GrillBot.Data.Models.API.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using NSwag.Annotations;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/auditlog")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
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
    [OpenApiOperation(nameof(AuditLogController) + "_" + nameof(RemoveItemAsync))]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
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
    [OpenApiOperation(nameof(AuditLogController) + "_" + nameof(GetAuditLogListAsync))]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<PaginatedResponse<AuditLogListItem>>> GetAuditLogListAsync([FromQuery] AuditLogListParams parameters, CancellationToken cancellationToken)
    {
        var result = await ApiService.GetListAsync(parameters, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets file that stored in log.
    /// </summary>
    /// <response code="200">Success.</response>
    /// <response code="404">Item not found or file not exists.</response>
    [HttpGet("{id}/{fileId}")]
    [OpenApiOperation(nameof(AuditLogController) + "_" + nameof(GetFileContentAsync))]
    [ProducesResponseType(typeof(FileContentResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> GetFileContentAsync(long id, long fileId, CancellationToken cancellationToken)
    {
        try
        {
            var fileInfo = await ApiService.GetLogItemFileAsync(id, fileId, cancellationToken);

            var contentType = new FileExtensionContentTypeProvider()
                .TryGetContentType(fileInfo.FullName, out var _contentType) ? _contentType : "application/octet-stream";

            var content = await System.IO.File.ReadAllBytesAsync(fileInfo.FullName, cancellationToken);
            return File(content, contentType);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new MessageResponse(ex.Message));
        }
    }
}
