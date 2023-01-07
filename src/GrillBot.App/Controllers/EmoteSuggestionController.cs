using System.Diagnostics.CodeAnalysis;
using GrillBot.App.Actions;
using GrillBot.Data.Models.API.Suggestions;
using GrillBot.Database.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace GrillBot.App.Controllers;

[ApiController]
[Route("api/emotes/suggestion")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
[ApiExplorerSettings(GroupName = "v1")]
[ExcludeFromCodeCoverage]
public class EmoteSuggestionController : Controller
{
    private IServiceProvider ServiceProvider { get; }

    public EmoteSuggestionController(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
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
        ApiAction.Init(this, parameters);

        var action = ServiceProvider.GetRequiredService<Actions.Api.V1.Emote.GetEmoteSuggestionsList>();
        var result = await action.ProcessAsync(parameters);
        return Ok(result);
    }
}
