using GrillBot.App.Services.Suggestion;
using GrillBot.Data.Models.API.Suggestions;
using GrillBot.Database.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/emotes/suggestion")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public class EmoteSuggestionController : Controller
{
    private EmoteSuggestionApiService ApiService { get; }

    public EmoteSuggestionController(EmoteSuggestionApiService apiService)
    {
        ApiService = apiService;
    }

    /// <summary>
    /// Get paginated list of emote sugestions.
    /// </summary>
    /// <response code="200">Return paginated list of emote suggestions</response>
    /// <response code="400">Validation of parameters failed</response>
    [HttpPost("list")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedResponse<EmoteSuggestion>>> GetSuggestionListAsync([FromBody] GetSuggestionsListParams parameters)
    {
        this.StoreParameters(parameters);
        
        var result = await ApiService.GetListAsync(parameters);
        return Ok(result);
    }
}
