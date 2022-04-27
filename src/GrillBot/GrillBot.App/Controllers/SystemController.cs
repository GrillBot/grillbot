using GrillBot.App.Services.Discord;
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
    private DiscordInitializationService InitializationService { get; }

    public SystemController(IWebHostEnvironment environment, DiscordSocketClient discordClient,
        DiscordInitializationService initializationService)
    {
        Environment = environment;
        DiscordClient = discordClient;
        InitializationService = initializationService;
    }

    /// <summary>
    /// Gets diagnostics data about application.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpGet("diag")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public ActionResult<DiagnosticsInfo> GetDiagnostics()
    {
        var data = new DiagnosticsInfo(Environment.EnvironmentName, DiscordClient, InitializationService.Get());
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
        InitializationService.Set(isActive);
        return Ok();
    }
}
