using GrillBot.App.Actions;
using GrillBot.App.Actions.Api.V1.Emote;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Validation;
using GrillBot.Data.Models.API.Emotes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/emotes")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[ApiExplorerSettings(GroupName = "v1")]
public class EmotesController : Infrastructure.ControllerBase
{
    public EmotesController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <summary>
    /// Get statistics of emotes.
    /// </summary>
    /// <response code="200">Return paginated list with statistics of emotes.</response>
    /// <response code="400">Validation of parameters failed.</response>
    [HttpPost("stats/list/{unsupported}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedResponse<EmoteStatItem>>> GetStatsOfEmotesAsync([FromBody] EmotesListParams @params, bool unsupported)
    {
        ApiAction.Init(this, @params);
        return Ok(await ProcessActionAsync<GetStatsOfEmotes, PaginatedResponse<EmoteStatItem>>(action => action.ProcessAsync(@params, unsupported)));
    }

    /// <summary>
    /// Merge statistics between emotes.
    /// </summary>
    /// <response code="200">Returns count of changed rows in the database.</response>
    /// <response code="400">Validation of parameters failed.</response>
    [HttpPost("stats/merge")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<int>> MergeStatsToAnotherAsync([FromBody] MergeEmoteStatsParams @params)
    {
        ApiAction.Init(this, @params);
        return Ok(await ProcessActionAsync<MergeStats, int>(action => action.ProcessAsync(@params)));
    }

    /// <summary>
    /// Remove statstics of emote.
    /// </summary>
    /// <response code="200">Returns count of changed rows in the database.</response>
    /// <response code="400">Validation of EmoteId failed.</response>
    [HttpDelete("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<int>> RemoveStatisticsAsync(
        [Required(ErrorMessage = "Pro smazání je vyžadováno EmoteId.")] [EmoteId(ErrorMessage = "Zadaný vstup není EmoteId.")]
        string emoteId
    ) => Ok(await ProcessActionAsync<RemoveStats, int>(action => action.ProcessAsync(emoteId)));

    /// <summary>
    /// Get a paginated list of users who use a specific emote.
    /// </summary>
    /// <response code="200">Returns paginated list of users.</response>
    /// <response code="400">Validation of parameters failed.</response>
    [HttpPost("list/users")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedResponse<EmoteStatsUserListItem>>> GetUserStatisticsOfEmoteAsync([FromBody] EmoteStatsUserListParams parameters)
        => Ok(await ProcessActionAsync<GetUserStatisticsOfEmote, PaginatedResponse<EmoteStatsUserListItem>>(action => action.ProcessAsync(parameters)));

    /// <summary>
    /// Get statistics of one emote.
    /// </summary>
    /// <response code="200">Returns statistics of emote.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="404">Emote not found.</response>
    [HttpGet("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EmoteStatItem>> GetStatOfEmoteAsync(
        [Required(ErrorMessage = "Je vyžadování EmoteId.")] [EmoteId(ErrorMessage = "Zadaný vstup není EmoteId.")]
        string emoteId
    ) => Ok(await ProcessActionAsync<GetStatOfEmote, EmoteStatItem>(action => action.ProcessAsync(emoteId)));
}
