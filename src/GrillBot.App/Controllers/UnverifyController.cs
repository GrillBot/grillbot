using GrillBot.Data.Models.API.Unverify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using GrillBot.Data.Models.API;
using GrillBot.App.Actions;
using GrillBot.App.Actions.Api.V1.Unverify;
using GrillBot.Core.Models.Pagination;
using Microsoft.AspNetCore.Http;

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
    [ProducesResponseType(typeof(List<UnverifyUserProfile>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentUnverifiesAsync()
        => await ProcessAsync<GetCurrentUnverifies>();

    /// <summary>
    /// Removes unverify
    /// </summary>
    /// <response code="200">Success</response>
    /// <response code="404">Unverify or guild not found.</response>
    [HttpDelete("{guildId}/{userId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveUnverifyAsync(ulong guildId, ulong userId, bool force = false)
        => await ProcessAsync<RemoveUnverify>(guildId, userId, force);

    /// <summary>
    /// Update unverify time.
    /// </summary>
    [HttpPut("{guildId}/{userId}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUnverifyTimeAsync(ulong guildId, ulong userId, UpdateUnverifyParams parameters)
    {
        ApiAction.Init(this, parameters);
        return await ProcessAsync<UpdateUnverify>(guildId, userId, parameters);
    }

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

    /// <summary>
    /// Recover state before specific unverify.
    /// </summary>
    /// <param name="logId">ID of log.</param>
    /// <response code="200">Success</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="404">Unverify, guild or users not found.</response>
    [HttpPost("log/{logId:long}/recover")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RecoverUnverifyAsync(long logId)
        => await ProcessAsync<RecoverState>(logId);
}
