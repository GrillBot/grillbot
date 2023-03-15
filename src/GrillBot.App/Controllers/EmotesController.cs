using GrillBot.App.Actions;
using GrillBot.Core.Models.Pagination;
using GrillBot.Core.Validation;
using GrillBot.Data.Models.API.Emotes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/emotes")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[ApiExplorerSettings(GroupName = "v1")]
public class EmotesController : Controller
{
    private IServiceProvider ServiceProvider { get; }

    public EmotesController(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Get statistics of supported emotes.
    /// </summary>
    /// <response code="200">Return paginated list with statistics of supported emotes.</response>
    /// <response code="400">Validation of parameters failed.</response>
    [HttpPost("stats/supported/list")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedResponse<EmoteStatItem>>> GetStatsOfSupportedEmotesAsync([FromBody] EmotesListParams @params)
    {
        ApiAction.Init(this, @params);

        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Emote.GetStatsOfEmotes>();
        var result = await action.ProcessAsync(@params, false);
        return Ok(result);
    }

    /// <summary>
    /// Get statistics of unsupported emotes.
    /// </summary>
    /// <response code="200">Return paginated list with statistics of unsupported emotes.</response>
    /// <response code="400">Validation of parameters failed.</response>
    [HttpPost("stats/unsupported/list")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedResponse<EmoteStatItem>>> GetStatsOfUnsupportedEmotesAsync([FromBody] EmotesListParams @params)
    {
        ApiAction.Init(this, @params);

        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Emote.GetStatsOfEmotes>();
        var result = await action.ProcessAsync(@params, true);
        return Ok(result);
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

        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Emote.MergeStats>();
        var result = await action.ProcessAsync(@params);
        return Ok(result);
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
    )
    {
        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Emote.RemoveStats>();
        var result = await action.ProcessAsync(emoteId);
        return Ok(result);
    }
}
