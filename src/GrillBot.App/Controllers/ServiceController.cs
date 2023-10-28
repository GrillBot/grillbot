using GrillBot.App.Actions.Api;
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
    [ProducesResponseType(typeof(ServiceInfo), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetServiceInfoAsync(string id)
        => await ProcessAsync<Actions.Api.V1.Services.GetServiceInfo>(id);

    /// <summary>
    /// Get additional status info of AuditLogService. 
    /// </summary>
    /// <response code="200">Returns additional status info.</response>
    [HttpGet("auditLog/status")]
    [ProducesResponseType(typeof(StatusInfo), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuditLogStatusInfoAsync()
    {
        var executor = (IAuditLogServiceClient client) => client.GetStatusInfoAsync();
        return await ProcessAsync<ServiceBridgeAction<IAuditLogServiceClient>>(executor);
    }

    /// <summary>
    /// Get additional status info of PointsService.
    /// </summary>
    /// <response code="200">Returns additional status info.</response>
    [HttpGet("points/status")]
    [ProducesResponseType(typeof(Core.Services.PointsService.Models.StatusInfo), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPointsServiceSatusInfoAsync()
    {
        var executor = (IPointsServiceClient client) => client.GetStatusInfoAsync();
        return await ProcessAsync<ServiceBridgeAction<IPointsServiceClient>>(executor);
    }
}
