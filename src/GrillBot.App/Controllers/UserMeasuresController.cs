using GrillBot.App.Actions;
using GrillBot.App.Actions.Api.V1.UserMeasures;
using GrillBot.Core.Models.Pagination;
using GrillBot.Data.Models.API.UserMeasures;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers;

[Route("api/user/measures")]
[ApiExplorerSettings(GroupName = "v1")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public class UserMeasuresController : Core.Infrastructure.Actions.ControllerBase
{
    public UserMeasuresController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <summary>
    /// Get paginated list of user measures.
    /// </summary>
    /// <response code="200">Returns paginated list of user measures.</response>
    /// <response code="400">Validation of parameters failed.</response>
    [HttpPost("list")]
    [ProducesResponseType(typeof(PaginatedResponse<UserMeasuresListItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUserMeasuresListAsync([FromBody] UserMeasuresParams parameters)
    {
        ApiAction.Init(this, parameters);
        return await ProcessAsync<GetUserMeasuresList>(parameters);
    }

    /// <summary>
    /// Create new member warning.
    /// </summary>
    /// <response code="200">Creates new member warning</response>
    /// <response code="400">Validation of parameters failed.</response>
    [HttpPost("create")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateUserMeasuresWarningAsync([FromBody] CreateUserMeasuresWarningParams parameters)
    {
        ApiAction.Init(this, parameters);
        return await ProcessAsync<CreateUserMeasuresWarning>(parameters);
    }
}
