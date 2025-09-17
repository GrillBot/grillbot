using GrillBot.Data.Models.API.Unverify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using GrillBot.App.Actions;
using GrillBot.App.Actions.Api.V1.Unverify;
using GrillBot.Core.Models.Pagination;
using Microsoft.AspNetCore.Http;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/unverify")]
[ApiExplorerSettings(GroupName = "v1")]
public class UnverifyController(IServiceProvider serviceProvider) : Core.Infrastructure.Actions.ControllerBase(serviceProvider)
{

    /// <summary>
    /// Get list of current unverifies in guild.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpGet("current")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
    [ProducesResponseType(typeof(List<UnverifyUserProfile>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentUnverifiesAsync()
        => await ProcessAsync<GetCurrentUnverifies>();

    /// <summary>
    /// Gets paginated list of unverify logs.
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="400">Validation failed</response>
    [HttpPost("log")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
    [ProducesResponseType(typeof(PaginatedResponse<UnverifyLogItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUnverifyLogsAsync([FromBody] UnverifyLogParams parameters)
    {
        ApiAction.Init(this, parameters);
        return await ProcessAsync<GetLogs>(parameters);
    }
}
