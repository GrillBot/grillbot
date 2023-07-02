using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/system")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[ApiExplorerSettings(GroupName = "v1")]
public class SystemController : Infrastructure.ControllerBase
{
    public SystemController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <summary>
    /// Changes bot account status and set bot's status activity.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpPut("status")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public ActionResult ChangeBotStatus(bool isActive)
    {
        ProcessAction<Actions.Api.V1.System.ChangeBotStatus>(action => action.Process(isActive));
        return Ok();
    }

    /// <summary>
    /// Gets list of discord event logs.
    /// </summary>
    /// <response code="200">Returns last 1000 events from discord.</response>
    [HttpGet("eventLog")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<string[]> GetEventLog()
        => Ok(ProcessAction<Actions.Api.V1.System.GetEventLog, string[]>(action => action.Process()));
}
