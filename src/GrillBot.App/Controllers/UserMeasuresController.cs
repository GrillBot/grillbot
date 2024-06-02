using GrillBot.App.Actions;
using GrillBot.App.Actions.Api;
using GrillBot.App.Actions.Api.V1.UserMeasures;
using GrillBot.App.Actions.Api.V2.User;
using GrillBot.App.Infrastructure.Auth;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Services.UserMeasures;
using GrillBot.Core.Services.UserMeasures.Models.Measures;
using GrillBot.Data.Models.API.UserMeasures;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers;

[Route("api/user/measures")]
[ApiExplorerSettings(GroupName = "v1")]
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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public async Task<IActionResult> GetUserMeasuresListAsync([FromBody] MeasuresListParams parameters)
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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public async Task<IActionResult> CreateUserMeasuresWarningAsync([FromBody] CreateMemberWarningParams parameters)
    {
        ApiAction.Init(this, parameters);
        return await ProcessAsync<CreateUserMeasuresWarning>(parameters);
    }

    /// <summary>
    /// Create or update member timeout.
    /// </summary>
    /// <param name="parameters">Timeout parameters</param>
    /// <response code="200">Timeout has been successfully created.</response>
    /// <response code="400">Validation of parameters failed.</response>
    [ApiKeyAuth]
    [ApiExplorerSettings(GroupName = "v2")]
    [HttpPost("timeout/create")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUserMeasuresTimeoutAsync([FromBody] CreateUserMeasuresTimeoutParams parameters)
    {
        ApiAction.Init(this, parameters);
        return await ProcessAsync<CreateUserMeasuresTimeout>(parameters);
    }

    /// <summary>
    /// Delete member timeout.
    /// </summary>
    /// <param name="timeoutId">Timeout ID from external system.</param>
    /// <response code="200">Timeout has been successfully deleted.</response>
    /// <response code="400">Validation of parameters failed.</response>
    /// <response code="404">Timeout wasn't found.</response>
    [ApiKeyAuth]
    [ApiExplorerSettings(GroupName = "v2")]
    [HttpDelete("timeout/{timeoutId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUserMeasureTimeoutAsync(long timeoutId)
    {
        var executor = new Func<IUserMeasuresServiceClient, Task>(
            (IUserMeasuresServiceClient client) => client.DeleteMeasureAsync(DeleteMeasuresRequest.FromExternalSystem(timeoutId, "Timeout"))
        );

        return await ProcessAsync<ServiceBridgeAction<IUserMeasuresServiceClient>>(executor);
    }
}
