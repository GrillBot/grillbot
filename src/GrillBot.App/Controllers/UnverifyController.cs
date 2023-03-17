using GrillBot.Data.Models.API.Unverify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using GrillBot.Data.Models.API;
using GrillBot.App.Actions;
using GrillBot.App.Actions.Api.V1.Unverify;
using GrillBot.Core.Models.Pagination;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/unverify")]
[ApiExplorerSettings(GroupName = "v1")]
public class UnverifyController : Infrastructure.ControllerBase
{
    public UnverifyController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <summary>
    /// Get list of current unverifies in guild.
    /// </summary>
    /// <response code="200">Success</response>
    [HttpGet("current")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<ActionResult<List<UnverifyUserProfile>>> GetCurrentUnverifiesAsync()
        => Ok(await ProcessActionAsync<GetCurrentUnverifies, List<UnverifyUserProfile>>(action => action.ProcessAsync()));

    /// <summary>
    /// Removes unverify
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="404">Unverify or guild not found.</response>
    [HttpDelete("{guildId}/{userId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<MessageResponse>> RemoveUnverifyAsync(ulong guildId, ulong userId, bool force = false)
    {
        return Ok(new MessageResponse(
            await ProcessActionAsync<RemoveUnverify, string>(action => action.ProcessAsync(guildId, userId, force))
        ));
    }

    /// <summary>
    /// Update unverify time.
    /// </summary>
    [HttpPut("{guildId}/{userId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
    public async Task<ActionResult<MessageResponse>> UpdateUnverifyTimeAsync(ulong guildId, ulong userId, UpdateUnverifyParams parameters)
    {
        ApiAction.Init(this, parameters);

        var result = await ProcessActionAsync<UpdateUnverify, string>(action => action.ProcessAsync(guildId, userId, parameters));
        return Ok(new MessageResponse(result));
    }

    /// <summary>
    /// Gets paginated list of unverify logs.
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="400">Validation failed</response>
    [HttpPost("log")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,User")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult<PaginatedResponse<UnverifyLogItem>>> GetUnverifyLogsAsync([FromBody] UnverifyLogParams parameters)
    {
        ApiAction.Init(this, parameters);
        return Ok(await ProcessActionAsync<GetLogs, PaginatedResponse<UnverifyLogItem>>(action => action.ProcessAsync(parameters)));
    }

    /// <summary>
    /// Recover state before specific unverify.
    /// </summary>
    /// <param name="logId">ID of log.</param>
    /// <response code="200">Success</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="404">Unverify, guild or users not found.</response>
    [HttpPost("log/{logId:long}/recover")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(MessageResponse), (int)HttpStatusCode.NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), (int)HttpStatusCode.BadRequest)]
    public async Task<ActionResult> RecoverUnverifyAsync(long logId)
    {
        await ProcessActionAsync<RecoverState>(action => action.ProcessAsync(logId));
        return Ok();
    }
}
