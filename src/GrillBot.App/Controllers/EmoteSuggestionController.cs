using GrillBot.App.Actions;
using GrillBot.App.Actions.Api.V1.Emote;
using GrillBot.Core.Models.Pagination;
using GrillBot.Data.Models.API.Suggestions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/emotes/suggestion")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[ApiExplorerSettings(GroupName = "v1")]
public class EmoteSuggestionController : Infrastructure.ControllerBase
{
    public EmoteSuggestionController(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <summary>
    /// Get paginated list of emote sugestions.
    /// </summary>
    /// <response code="200">Return paginated list of emote suggestions</response>
    /// <response code="400">Validation of parameters failed</response>
    [HttpPost("list")]
    [ProducesResponseType(typeof(PaginatedResponse<EmoteSuggestion>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSuggestionListAsync([FromBody] GetSuggestionsListParams parameters)
    {
        ApiAction.Init(this, parameters);
        return await ProcessAsync<GetEmoteSuggestionsList>(parameters);
    }
}
