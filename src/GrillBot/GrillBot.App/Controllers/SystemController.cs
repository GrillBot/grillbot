using GrillBot.Common.Managers;
using GrillBot.Common.Managers.Counters;
using GrillBot.Data.Models.API.System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/system")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[ApiExplorerSettings(GroupName = "v1")]
public class SystemController : Controller
{
    private IWebHostEnvironment Environment { get; }
    private DiscordSocketClient DiscordClient { get; }
    private InitManager InitManager { get; }
    private CounterManager CounterManager { get; }
    private EventManager EventManager { get; }

    public SystemController(IWebHostEnvironment environment, DiscordSocketClient discordClient,
        InitManager initManager, CounterManager counterManager, EventManager eventManager)
    {
        Environment = environment;
        DiscordClient = discordClient;
        InitManager = initManager;
        CounterManager = counterManager;
        EventManager = eventManager;
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

    /// <summary>
    /// Gets list of discord event logs.
    /// </summary>
    /// <response code="200">Returns last 1000 events from discord.</response>
    [HttpGet("eventLog")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<string[]> GetEventLog()
    {
        var data = EventManager.GetEventLog();
        return Ok(data);
    }
}
