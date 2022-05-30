using GrillBot.Common.Managers;
using GrillBot.Common.Managers.Counters;
using GrillBot.Data.Models.API.System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/system")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[OpenApiTag("System", Description = "Internal system management, ...")]
public class SystemController : Controller
{
    private IWebHostEnvironment Environment { get; }
    private DiscordSocketClient DiscordClient { get; }
    private InitManager InitManager { get; }
    private CounterManager CounterManager { get; }

    public SystemController(IWebHostEnvironment environment, DiscordSocketClient discordClient,
        InitManager initManager, CounterManager counterManager)
    {
        Environment = environment;
        DiscordClient = discordClient;
        InitManager = initManager;
        CounterManager = counterManager;
    }

    /// <summary>
    /// Gets diagnostics data about application.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpGet("diag")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public ActionResult<DiagnosticsInfo> GetDiagnostics()
    {
        var isActive = InitManager.Get();
        var activeOperations = CounterManager.GetActiveCounters();

        var data = new DiagnosticsInfo(Environment.EnvironmentName, DiscordClient, isActive, activeOperations);
        return Ok(data);
    }

    /// <summary>
    /// Changes bot account status and set bot's status activity.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpPut("status")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public ActionResult ChangeBotStatus(bool isActive)
    {
        InitManager.Set(isActive);
        return Ok();
    }
}
