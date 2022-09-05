using GrillBot.App.Services.Emotes;
using GrillBot.Data.Infrastructure.Validation;
using GrillBot.Data.Models.API.Emotes;
using GrillBot.Database.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/emotes")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public class EmotesController : Controller
{
    private EmotesApiService EmotesApiService { get; }

    public EmotesController(EmotesApiService emotesApiService)
    {
        EmotesApiService = emotesApiService;
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
        this.StoreParameters(@params);

        var result = await EmotesApiService.GetStatsOfEmotesAsync(@params, false);
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
        this.StoreParameters(@params);

        var result = await EmotesApiService.GetStatsOfEmotesAsync(@params, true);
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
        try
        {
            this.StoreParameters(@params);
            var result = await EmotesApiService.MergeStatsToAnotherAsync(@params);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            var result = ex.ValidationResult;
            ModelState.AddModelError(result.MemberNames.First(), result.ErrorMessage!);

            return BadRequest(new ValidationProblemDetails(ModelState));
        }
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
        var result = await EmotesApiService.RemoveStatisticsAsync(emoteId);
        return Ok(result);
    }
}
