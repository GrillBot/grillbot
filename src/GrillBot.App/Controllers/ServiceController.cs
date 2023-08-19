using GrillBot.Core.Services.AuditLog;
using GrillBot.Core.Services.AuditLog.Models.Response.Info;
using GrillBot.Core.Services.PointsService;
using GrillBot.Data.Models.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ControllerBase = GrillBot.App.Infrastructure.ControllerBase;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/service")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[ApiExplorerSettings(GroupName = "v1")]
public class ServiceController : ControllerBase
{
    public ServiceController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <summary>
    /// Get info about service.
    /// </summary>
    /// <response code="200">Returns service info.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ServiceInfo>> GetServiceInfoAsync(string id)
        => Ok(await ProcessActionAsync<Actions.Api.V1.Services.GetServiceInfo, ServiceInfo>(action => action.ProcessAsync(id)));

    /// <summary>
    /// Get additional status info of AuditLogService. 
    /// </summary>
    /// <response code="200">Returns additional status info.</response>
    [HttpGet("auditLog/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<StatusInfo>> GetAuditLogStatusInfoAsync()
        => Ok(await ProcessBridgeAsync<IAuditLogServiceClient, StatusInfo>(client => client.GetStatusInfoAsync()));

    /// <summary>
    /// Get additional status info of PointsService.
    /// </summary>
    /// <response code="200">Returns additional status info.</response>
    [HttpGet("points/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Core.Services.PointsService.Models.StatusInfo>> GetPointsServiceSatusInfoAsync()
        => Ok(await ProcessBridgeAsync<IPointsServiceClient, Core.Services.PointsService.Models.StatusInfo>(client => client.GetStatusInfoAsync()));
}
