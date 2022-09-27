using System.Diagnostics.CodeAnalysis;
using GrillBot.Data.Models.API.System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/system")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[ApiExplorerSettings(GroupName = "v1")]
[ExcludeFromCodeCoverage]
public class SystemController : Controller
{
    private IServiceProvider ServiceProvider { get; }

    public SystemController(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Gets diagnostics data about application.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpGet("diag")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public ActionResult<DiagnosticsInfo> GetDiagnostics()
    {
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.System.GetDiagnostics>();
        var result = action.Process();

        return Ok(result);
    }

    /// <summary>
    /// Changes bot account status and set bot's status activity.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpPut("status")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public ActionResult ChangeBotStatus(bool isActive)
    {
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.System.ChangeBotStatus>();
        action.Process(isActive);
        return Ok();
    }

    /// <summary>
    /// Gets list of discord event logs.
    /// </summary>
    /// <response code="200">Returns last 1000 events from discord.</response>
    [HttpGet("eventLog")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<string[]> GetEventLog()
    {
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.System.GetEventLog>();
        var result = action.Process();
        return Ok(result);
    }
}
