using GrillBot.App.Actions;
using GrillBot.App.Actions.Api;
using GrillBot.App.Actions.Api.V2.User;
using GrillBot.App.Infrastructure.Auth;
using GrillBot.Core.Services.Common.Executor;
using UserMeasures;
using UserMeasures.Models.Measures;
using GrillBot.Data.Models.API.UserMeasures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers;

[ApiKeyAuth]
[Route("api/user/measures")]
[ApiExplorerSettings(GroupName = "v2")]
public class UserMeasuresController : Core.Infrastructure.Actions.ControllerBase
{
    public UserMeasuresController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <summary>
    /// Create or update member timeout.
    /// </summary>
    /// <param name="parameters">Timeout parameters</param>
    /// <response code="200">Timeout has been successfully created.</response>
    /// <response code="400">Validation of parameters failed.</response>
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
    [HttpDelete("timeout/{timeoutId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUserMeasureTimeoutAsync(long timeoutId)
    {
        var executor = new Func<IUserMeasuresServiceClient, ServiceExecutorContext, Task>(
            (client, ctx) => client.DeleteMeasureAsync(DeleteMeasuresRequest.FromExternalSystem(timeoutId, "Timeout"), ctx.CancellationToken)
        );

        return await ProcessAsync<ServiceBridgeAction<IUserMeasuresServiceClient>>(executor);
    }
}
